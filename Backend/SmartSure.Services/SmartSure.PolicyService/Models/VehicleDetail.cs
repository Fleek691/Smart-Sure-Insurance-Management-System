namespace SmartSure.PolicyService.Models;

/// <summary>
/// Vehicle-specific details captured at policy purchase time.
/// One-to-one with <see cref="Policy"/> — only present for Vehicle insurance policies.
/// </summary>
public class VehicleDetail
{
    public Guid VehicleId { get; set; }
    public Guid PolicyId { get; set; }

    /// <summary>Registration number of the insured vehicle.</summary>
    public string VehicleNumber { get; set; } = string.Empty;

    /// <summary>Make and model of the vehicle, e.g. "Maruti Suzuki Swift".</summary>
    public string Model { get; set; } = string.Empty;

    public int YearOfMake { get; set; }

    /// <summary>Engine displacement in cubic centimetres — optional for electric vehicles.</summary>
    public int? EngineCC { get; set; }

    public Policy? Policy { get; set; }
}