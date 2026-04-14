using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Services;
using SmartSure.Shared.Constants;
using SmartSure.Shared.Exceptions;

namespace SmartSure.PolicyService.Controllers;

/// <summary>
/// Manages insurance products, policy purchases, Razorpay payments, and admin policy operations.
/// </summary>
[ApiController]
[Route("api/policies")]
public class PoliciesController(IPolicyService policyService, IRazorpayService razorpayService) : ControllerBase
{
    private readonly IPolicyService _policyService = policyService;
    private readonly IRazorpayService _razorpayService = razorpayService;

    /// <summary>Returns all available insurance products (public, no auth required).</summary>
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

    /// <summary>Admin-only: creates a new insurance product under a type/subtype.</summary>
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

    /// <summary>Customer: calculates the monthly premium for a product, coverage, and term.</summary>
    [HttpGet("calculate-premium")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<PremiumCalculationDto> CalculatePremium([FromQuery] int productId, [FromQuery] decimal coverageAmount, [FromQuery] int termMonths)
    {
        return await _policyService.CalculatePremiumAsync(productId, coverageAmount, termMonths);
    }

    /// <summary>Customer: purchases a policy directly (non-Razorpay flow).</summary>
    [HttpPost("purchase")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<PolicyDto> Purchase([FromBody] PurchasePolicyDto dto)
    {
        var userId = GetUserId();
        return await _policyService.PurchasePolicyAsync(userId, dto);
    }

    // ── Razorpay payment flow ─────────────────────────────────────────────────

    /// <summary>
    /// Step 1 — Create a Razorpay order.
    /// Returns the order id, amount in paise, and the publishable key so the
    /// frontend can open the Razorpay checkout widget.
    /// </summary>
    [HttpPost("payment/create-order")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<PaymentOrderResponseDto> CreatePaymentOrder([FromBody] CreatePaymentOrderDto dto)
    {
        var userId = GetUserId();
        return await _razorpayService.CreateOrderAsync(userId, dto);
    }

    /// <summary>
    /// Step 2 — Verify the Razorpay signature and activate the policy.
    /// Call this after the Razorpay checkout succeeds in the browser.
    /// </summary>
    [HttpPost("payment/verify")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<PolicyDto> VerifyPayment([FromBody] VerifyPaymentDto dto)
    {
        var userId = GetUserId();
        return await _razorpayService.VerifyAndActivateAsync(userId, dto);
    }

    /// <summary>Customer: returns all policies owned by the authenticated user.</summary>
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

    /// <summary>Customer: cancels an active policy and publishes a PolicyCancelledEvent.</summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<PolicyDto> Cancel(Guid id)
    {
        var userId = GetUserId();
        return await _policyService.CancelPolicyAsync(id, userId);
    }

    /// <summary>Admin-only: returns all policies across all users.</summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<List<PolicyDto>> GetAllForAdmin()
    {
        return await _policyService.GetAllPoliciesForAdminAsync();
    }

    /// <summary>Admin-only: updates a policy's lifecycle status.</summary>
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

    /// <summary>Verifies the caller owns the policy or is an admin.</summary>
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