namespace SmartSure.ClaimsService.Services;

/// <summary>
/// Abstracts file storage for claim documents.
/// Returns a (fileId, fileUrl) tuple that is persisted on the <see cref="Models.ClaimDocument"/>.
/// </summary>
public interface IMegaStorageService
{
    /// <summary>
    /// Uploads <paramref name="fileContent"/> to the configured storage backend.
    /// Returns the storage-specific file ID and a public download URL.
    /// </summary>
    Task<(string fileId, string fileUrl)> UploadAsync(string fileName, byte[] fileContent);
}