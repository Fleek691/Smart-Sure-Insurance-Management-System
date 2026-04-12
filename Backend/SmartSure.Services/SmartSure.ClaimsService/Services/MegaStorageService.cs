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
        // ── 1. Try Mega.nz ────────────────────────────────────────────────
        var email    = _configuration["Mega:Email"]    ?? string.Empty;
        var password = _configuration["Mega:Password"] ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            try
            {
                var client = new MegaApiClient();
                await client.LoginAsync(email, password);

                try
                {
                    var nodes        = await client.GetNodesAsync();
                    var root         = nodes.Single(x => x.Type == NodeType.Root);
                    using var stream = new MemoryStream(fileContent);
                    var node         = await client.UploadAsync(stream, fileName, root);
                    var link         = await client.GetDownloadLinkAsync(node);
                    _logger.LogInformation("Document uploaded to Mega: {FileName}", fileName);
                    return (node.Id, link.ToString());
                }
                finally
                {
                    await client.LogoutAsync();
                }
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
