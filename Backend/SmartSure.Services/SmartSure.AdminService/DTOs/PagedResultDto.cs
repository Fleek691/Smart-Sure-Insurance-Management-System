namespace SmartSure.AdminService.DTOs;

/// <summary>
/// Generic wrapper for paginated list responses.
/// Used by the audit log endpoint to support server-side pagination.
/// </summary>
public class PagedResultDto<T>
{
    /// <summary>Current page number (1-based).</summary>
    public int Page { get; set; }

    public int PageSize { get; set; }

    /// <summary>Total number of matching records across all pages.</summary>
    public int TotalCount { get; set; }

    /// <summary>Items on the current page.</summary>
    public List<T> Items { get; set; } = new();
}
