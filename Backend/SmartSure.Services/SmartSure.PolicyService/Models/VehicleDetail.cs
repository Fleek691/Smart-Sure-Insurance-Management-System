namespace SmartSure.PolicyService.Models;

public class VehicleDetail
{
    public Guid VehicleId { get; set; }
    public Guid PolicyId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int YearOfMake { get; set; }
    public int? EngineCC { get; set; }

    public Policy? Policy { get; set; }
}