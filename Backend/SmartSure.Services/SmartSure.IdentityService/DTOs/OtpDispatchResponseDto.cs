namespace SmartSure.IdentityService.DTOs;

public class OtpDispatchResponseDto
{
    public string Message { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
