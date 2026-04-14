namespace SmartSure.ClaimsService.Models;

/// <summary>
/// A supporting document attached to a claim (e.g. photos, invoices, medical reports).
/// Files are stored in Mega.nz with a local filesystem fallback.
/// </summary>
public class ClaimDocument
{
    public Guid DocId { get; set; }
    public Guid ClaimId { get; set; }

    /// <summary>Original file name provided by the customer.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Mega.nz node ID — used to build the download link or delete the file.</summary>
    public string MegaNzFileId { get; set; } = string.Empty;

    /// <summary>Public download URL (Mega.nz link or local fallback URL).</summary>
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>MIME type or extension, e.g. "application/pdf" or "image/png".</summary>
    public string FileType { get; set; } = string.Empty;

    public int FileSizeKb { get; set; }
    public DateTime UploadedAt { get; set; }

    public Claim? Claim { get; set; }
}