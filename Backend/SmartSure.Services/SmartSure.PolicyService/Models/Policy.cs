namespace SmartSure.PolicyService.Models;

public class Policy
{
    public Guid PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public int TypeId { get; set; }
    public int SubTypeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public DateOnly InsuranceDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal MonthlyPremium { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public InsuranceType? Type { get; set; }
    public InsuranceSubType? SubType { get; set; }
    public PolicyDetail? PolicyDetail { get; set; }
    public VehicleDetail? VehicleDetail { get; set; }
    public HomeDetail? HomeDetail { get; set; }
    public ICollection<Premium> Premiums { get; set; } = new List<Premium>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}