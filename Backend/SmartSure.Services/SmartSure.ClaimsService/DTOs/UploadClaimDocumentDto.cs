using System.ComponentModel.DataAnnotations;

namespace SmartSure.ClaimsService.DTOs;

/// <summary>
/// Request body for uploading a supporting document to a Draft claim.
/// The file content must be Base64-encoded by the client before sending.
/// </summary>
public class UploadClaimDocumentDto
{
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>MIME type or extension, e.g. "application/pdf" or "image/jpeg".</summary>
    [Required]
    [MaxLength(50)]
    public string FileType { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int FileSizeKb { get; set; }

    /// <summary>Base64-encoded file content — decoded server-side before uploading to storage.</summary>
    [Required]
    public string ContentBase64 { get; set; } = string.Empty;
}