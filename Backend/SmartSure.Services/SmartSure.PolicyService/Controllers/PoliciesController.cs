using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Services;
using SmartSure.Shared.Constants;
using SmartSure.Shared.Exceptions;

namespace SmartSure.PolicyService.Controllers;

[ApiController]
[Route("api/policies")]
public class PoliciesController(IPolicyService policyService) : ControllerBase
{
    private readonly IPolicyService _policyService = policyService;

    [HttpGet("products")]
    [AllowAnonymous]
    public async Task<List<InsuranceProductDto>> GetProducts()
    {
        return await _policyService.GetProductsAsync();
    }

    [HttpGet("products/{id:int}")]
    [AllowAnonymous]
    public async Task<InsuranceProductDto> GetProductById(int id)
    {
        return await _policyService.GetProductByIdAsync(id);
    }

    [HttpPost("products")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<InsuranceProductDto> CreateProduct([FromBody] CreateInsuranceProductDto dto)
    {
        return await _policyService.CreateProductAsync(dto);
    }

    [HttpPut("products/{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<InsuranceProductDto> UpdateProduct(int id, [FromBody] UpdateInsuranceProductDto dto)
    {
        return await _policyService.UpdateProductAsync(id, dto);
    }

    [HttpDelete("products/{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _policyService.DeleteProductAsync(id);
        return NoContent();
    }

    [HttpGet("calculate-premium")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<PremiumCalculationDto> CalculatePremium([FromQuery] int productId, [FromQuery] decimal coverageAmount, [FromQuery] int termMonths)
    {
        return await _policyService.CalculatePremiumAsync(productId, coverageAmount, termMonths);
    }

    [HttpPost("purchase")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<PolicyDto> Purchase([FromBody] PurchasePolicyDto dto)
    {
        var userId = GetUserId();
        return await _policyService.PurchasePolicyAsync(userId, dto);
    }

    [HttpGet("my-policies")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<List<PolicyDto>> GetMyPolicies()
    {
        var userId = GetUserId();
        return await _policyService.GetMyPoliciesAsync(userId);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<PolicyDto> GetById(Guid id)
    {
        var policy = await _policyService.GetPolicyByIdAsync(id);
        EnsureReadAccess(policy);
        return policy;
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<PolicyDto> Cancel(Guid id)
    {
        var userId = GetUserId();
        return await _policyService.CancelPolicyAsync(id, userId);
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<List<PolicyDto>> GetAllForAdmin()
    {
        return await _policyService.GetAllPoliciesForAdminAsync();
    }

    [HttpPut("admin/{id:guid}/status")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<PolicyDto> UpdatePolicyStatus(Guid id, [FromBody] UpdatePolicyStatusDto dto)
    {
        return await _policyService.UpdatePolicyStatusAsync(id, dto.Status);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : throw new UnauthorizedException("User id claim not found.");
    }

    private void EnsureReadAccess(PolicyDto policy)
    {
        if (User.IsInRole(Roles.Admin))
        {
            return;
        }

        if (policy.UserId != GetUserId())
        {
            throw new UnauthorizedException("You are not allowed to view this policy.");
        }
    }
}