namespace SmartSure.ClaimsService.DTOs;

/// <summary>Response DTO for a document attached to a claim.</summary>
public class ClaimDocumentDto
{
    public Guid DocId { get; set; }
    public Guid ClaimId { get; set; }
    public string FileName { get; set; } = string.Empty;

    /// <summary>Mega.nz node ID used to reference or delete the file.</summary>
    public string MegaNzFileId { get; set; } = string.Empty;

    /// <summary>Public download URL (Mega.nz or local fallback).</summary>
    public string FileUrl { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;
    public int FileSizeKb { get; set; }
    public DateTime UploadedAt { get; set; }
}