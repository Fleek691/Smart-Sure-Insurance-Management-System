using CG.Web.MegaApiClient;

namespace SmartSure.ClaimsService.Services;

public class MegaStorageService : IMegaStorageService
{
    private readonly IConfiguration _configuration;

    public MegaStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<(string fileId, string fileUrl)> UploadAsync(string fileName, byte[] fileContent)
    {
        var email = _configuration["Mega:Email"] ?? throw new InvalidOperationException("Mega:Email is missing.");
        var password = _configuration["Mega:Password"] ?? throw new InvalidOperationException("Mega:Password is missing.");

        var client = new MegaApiClient();
        await client.LoginAsync(email, password);

        try
        {
            var nodes = await client.GetNodesAsync();
            var root = nodes.Single(x => x.Type == NodeType.Root);
            using var stream = new MemoryStream(fileContent);
            var uploadedNode = await client.UploadAsync(stream, fileName, root);
            var link = await client.GetDownloadLinkAsync(uploadedNode);
            return (uploadedNode.Id, link.ToString());
        }
        finally
        {
            await client.LogoutAsync();
        }
    }
}