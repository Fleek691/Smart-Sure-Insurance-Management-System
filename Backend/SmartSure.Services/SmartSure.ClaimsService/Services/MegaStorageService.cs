using CG.Web.MegaApiClient;
using Microsoft.Extensions.Logging;

namespace SmartSure.ClaimsService.Services;

/// <summary>
/// Tries to upload to Mega.nz first.
/// If Mega fails for any reason (quota, auth, network), falls back to local filesystem storage.
/// </summary>
public class MegaStorageService : IMegaStorageService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MegaStorageService> _logger;

    public MegaStorageService(
        IConfiguration configuration,
        IWebHostEnvironment env,
        ILogger<MegaStorageService> logger)
    {
        _configuration = configuration;
        _env = env;
        _logger = logger;
    }

    public async Task<(string fileId, string fileUrl)> UploadAsync(string fileName, byte[] fileContent)
    {
        // ── 1. Try Mega.nz (with a hard 15-second timeout) ───────────────
        var email    = _configuration["Mega:Email"]    ?? string.Empty;
        var password = _configuration["Mega:Password"] ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            try
            {
                var megaResult = await UploadToMegaAsync(fileName, fileContent, email, password, cts.Token);
                return megaResult;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Mega upload timed out for '{FileName}', falling back to local storage.", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Mega upload failed for '{FileName}', falling back to local storage.", fileName);
            }
        }
        else
        {
            _logger.LogWarning("Mega credentials not configured, using local storage.");
        }

        // ── 2. Fallback — local filesystem ────────────────────────────────
        return StoreLocally(fileName, fileContent);
    }

    private async Task<(string fileId, string fileUrl)> UploadToMegaAsync(
        string fileName, byte[] fileContent, string email, string password, CancellationToken ct)
    {
        var client = new MegaApiClient();
        await client.LoginAsync(email, password).WaitAsync(ct);

        try
        {
            var nodes        = await client.GetNodesAsync().WaitAsync(ct);
            var root         = nodes.Single(x => x.Type == NodeType.Root);
            using var stream = new MemoryStream(fileContent);
            var node         = await client.UploadAsync(stream, fileName, root).WaitAsync(ct);
            var link         = await client.GetDownloadLinkAsync(node).WaitAsync(ct);
            _logger.LogInformation("Document uploaded to Mega: {FileName}", fileName);
            return (node.Id, link.ToString());
        }
        finally
        {
            try { await client.LogoutAsync(); } catch { /* best-effort logout */ }
        }
    }

    private (string fileId, string fileUrl) StoreLocally(string fileName, byte[] fileContent)
    {
        var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var uniqueName = $"{Guid.NewGuid():N}_{SanitizeName(fileName)}";
        File.WriteAllBytes(Path.Combine(uploadsDir, uniqueName), fileContent);

        var baseUrl = _configuration["App:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5003";
        var fileUrl = $"{baseUrl}/uploads/{uniqueName}";

        _logger.LogInformation("Document stored locally: {UniqueName}", uniqueName);
        return (uniqueName, fileUrl);
    }

    private static string SanitizeName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}
