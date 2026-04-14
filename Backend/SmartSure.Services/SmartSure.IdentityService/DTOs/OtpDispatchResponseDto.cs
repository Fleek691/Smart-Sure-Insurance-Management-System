namespace SmartSure.IdentityService.DTOs;

/// <summary>
/// Returned when an OTP has been dispatched (registration or resend).
/// Tells the client when the OTP will expire so it can show a countdown.
/// </summary>
public class OtpDispatchResponseDto
{
    public string Message { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the OTP becomes invalid.</summary>
    public DateTime ExpiresAtUtc { get; set; }
}
