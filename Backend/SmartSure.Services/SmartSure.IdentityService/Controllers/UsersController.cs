using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.IdentityService.DTOs;
using SmartSure.IdentityService.Services;

namespace SmartSure.IdentityService.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IUserAdministrationService _userAdministrationService;

    public UsersController(IProfileService profileService, IUserAdministrationService userAdministrationService)
    {
        _profileService = profileService;
        _userAdministrationService = userAdministrationService;
    }

    [HttpGet("profile")]
    public async Task<ProfileDto> GetProfile()
    {
        var userId = GetUserId();
        return await _profileService.GetProfileAsync(userId);
    }

    [HttpPut("profile")]
    public async Task<ProfileDto> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        return await _profileService.UpdateProfileAsync(userId, dto);
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    public async Task<List<ProfileDto>> GetUsers()
    {
        return await _userAdministrationService.GetUsersAsync();
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromQuery] bool isActive)
    {
        await _userAdministrationService.UpdateUserStatusAsync(id, isActive);
        return Ok();
    }

    [HttpPut("{id:guid}/role")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromQuery] string role)
    {
        await _userAdministrationService.UpdateUserRoleAsync(id, role);
        return Ok();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : throw new InvalidOperationException("User id claim not found.");
    }
}