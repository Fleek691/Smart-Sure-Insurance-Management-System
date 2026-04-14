namespace SmartSure.AdminService.DTOs;

/// <summary>High-level KPI snapshot shown on the admin dashboard.</summary>
public class DashboardStatsDto
{
    /// <summary>Total distinct policies created in the last 365 days.</summary>
    public int TotalPolicies { get; set; }

    /// <summary>Total distinct claims filed in the last 365 days.</summary>
    public int TotalClaims { get; set; }

    /// <summary>Total revenue (sum of policy premiums) in the last 365 days.</summary>
    public decimal TotalRevenue { get; set; }
}
