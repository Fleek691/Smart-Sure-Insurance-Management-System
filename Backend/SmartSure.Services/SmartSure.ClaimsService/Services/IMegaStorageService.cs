namespace SmartSure.ClaimsService.Services;

public interface IMegaStorageService
{
    Task<(string fileId, string fileUrl)> UploadAsync(string fileName, byte[] fileContent);
}