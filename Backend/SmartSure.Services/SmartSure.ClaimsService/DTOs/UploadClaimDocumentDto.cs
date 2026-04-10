using System.ComponentModel.DataAnnotations;

namespace SmartSure.ClaimsService.DTOs;

public class UploadClaimDocumentDto
{
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string FileType { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int FileSizeKb { get; set; }

    [Required]
    public string ContentBase64 { get; set; } = string.Empty;
}