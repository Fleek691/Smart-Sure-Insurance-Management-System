namespace SmartSure.ClaimsService.DTOs;

public class ClaimDocumentDto
{
    public Guid DocId { get; set; }
    public Guid ClaimId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MegaNzFileId { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int FileSizeKb { get; set; }
    public DateTime UploadedAt { get; set; }
}