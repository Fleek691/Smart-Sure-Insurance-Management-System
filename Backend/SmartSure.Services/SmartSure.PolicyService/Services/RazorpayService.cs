using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Razorpay.Api;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Models;
using SmartSure.PolicyService.Repositories;
using SmartSure.Shared.Constants;
using SmartSure.Shared.Events;
using SmartSure.Shared.Exceptions;
using PolicyPayment = SmartSure.PolicyService.Models.Payment;

namespace SmartSure.PolicyService.Services;

/// <summary>
/// Handles the two-step Razorpay payment flow:
/// 1. <see cref="CreateOrderAsync"/> — creates a Razorpay order and caches the pending purchase.
/// 2. <see cref="VerifyAndActivateAsync"/> — verifies the HMAC signature, activates the policy, and records the payment.
/// </summary>
public class RazorpayService : IRazorpayService
{
    private const string PendingOrderCachePrefix = "razorpay_pending_";
    private const int PendingOrderTtlMinutes = 30;
    private readonly IConfiguration _configuration;
    private readonly IPolicyRepository _policyRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly IPolicyEventPublisher _eventPublisher;
    private readonly ILogger<RazorpayService> _logger;

    public RazorpayService(
        IConfiguration configuration,
        IPolicyRepository policyRepository,
        IMemoryCache memoryCache,
        IPolicyEventPublisher eventPublisher,
        ILogger<RazorpayService> logger)
    {
        _configuration = configuration;
        _policyRepository = policyRepository;
        _memoryCache = memoryCache;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    // ── Create Order ──────────────────────────────────────────────────────────

    /// <summary>
    /// Step 1 — Creates a Razorpay order for the given product and purchase parameters.
    /// Caches the pending purchase details (keyed by a random token) so they can be
    /// retrieved after the frontend completes payment and calls VerifyAndActivateAsync.
    /// </summary>
    public async Task<PaymentOrderResponseDto> CreateOrderAsync(Guid userId, CreatePaymentOrderDto dto)
    {
        ValidatePurchaseInput(dto.ProductId, dto.CoverageAmount, dto.TermMonths, dto.InsuranceDate);

        var product = await _policyRepository.GetProductByIdAsync(dto.ProductId)
                      ?? throw new NotFoundException("Insurance product not found.");

        var monthlyPremium = CalculatePremium(product.BasePremium, dto.CoverageAmount, dto.TermMonths);

        // Razorpay amount is in paise (1 INR = 100 paise)
        var amountPaise = (long)Math.Round(monthlyPremium * 100, 0);

        var keyId     = _configuration["Razorpay:KeyId"]     ?? throw new InvalidOperationException("Razorpay:KeyId is missing.");
        var keySecret = _configuration["Razorpay:KeySecret"] ?? throw new InvalidOperationException("Razorpay:KeySecret is missing.");

        var client = new RazorpayClient(keyId, keySecret);

        var orderOptions = new Dictionary<string, object>
        {
            ["amount"]   = amountPaise,
            ["currency"] = "INR",
            ["receipt"]  = $"ss_{userId:N}",
            ["notes"]    = new Dictionary<string, string>
            {
                ["userId"]    = userId.ToString(),
                ["productId"] = dto.ProductId.ToString()
            }
        };

        Order razorpayOrder;
        try
        {
            razorpayOrder = await Task.Run(() => client.Order.Create(orderOptions));
        }
        catch (Exception ex)
        {
            throw new HttpServiceException($"Failed to create Razorpay order: {ex.Message}", 502);
        }

        var razorpayOrderId = razorpayOrder["id"].ToString()!;

        // Cache the pending purchase so we can recreate it after payment verification
        var pendingToken = Guid.NewGuid().ToString("N");
        var pending = new PendingPurchase
        {
            UserId          = userId,
            ProductId       = dto.ProductId,
            CoverageAmount  = dto.CoverageAmount,
            TermMonths      = dto.TermMonths,
            InsuranceDate   = dto.InsuranceDate,
            MonthlyPremium  = monthlyPremium,
            AmountPaise     = amountPaise,
            RazorpayOrderId = razorpayOrderId
        };

        _memoryCache.Set(
            PendingOrderCachePrefix + pendingToken,
            pending,
            TimeSpan.FromMinutes(PendingOrderTtlMinutes));

        return new PaymentOrderResponseDto
        {
            RazorpayOrderId  = razorpayOrderId,
            AmountPaise      = amountPaise,
            Currency         = "INR",
            RazorpayKeyId    = keyId,
            MonthlyPremium   = monthlyPremium,
            PendingOrderToken = pendingToken
        };
    }

    // ── Verify & Activate ─────────────────────────────────────────────────────

    /// <summary>
    /// Step 2 — Verifies the Razorpay HMAC-SHA256 signature, activates the policy,
    /// records the payment, and publishes a PolicyActivatedEvent.
    /// The pending cache entry is removed after verification to prevent replay attacks.
    /// </summary>
    public async Task<PolicyDto> VerifyAndActivateAsync(Guid userId, VerifyPaymentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PendingOrderToken))
            throw new ValidationException("Pending order token is required.");

        if (string.IsNullOrWhiteSpace(dto.RazorpayOrderId))
            throw new ValidationException("Razorpay order id is required.");

        if (string.IsNullOrWhiteSpace(dto.RazorpayPaymentId))
            throw new ValidationException("Razorpay payment id is required.");

        if (string.IsNullOrWhiteSpace(dto.RazorpaySignature))
            throw new ValidationException("Razorpay signature is required.");

        // 1. Retrieve cached pending purchase
        var cacheKey = PendingOrderCachePrefix + dto.PendingOrderToken;
        if (!_memoryCache.TryGetValue(cacheKey, out PendingPurchase? pending) || pending is null)
        {
            throw new BusinessRuleException("Payment session expired or not found. Please start the purchase again.");
        }

        // 2. Verify the order id matches what we issued
        if (!string.Equals(pending.RazorpayOrderId, dto.RazorpayOrderId, StringComparison.Ordinal))
        {
            throw new BusinessRuleException("Order id mismatch. Payment cannot be verified.");
        }

        // 3. Verify HMAC-SHA256 signature
        //    Razorpay signs: razorpay_order_id + "|" + razorpay_payment_id
        var keySecret = _configuration["Razorpay:KeySecret"]
                        ?? throw new InvalidOperationException("Razorpay:KeySecret is missing.");

        var payload   = $"{dto.RazorpayOrderId}|{dto.RazorpayPaymentId}";
        var isValid   = VerifySignature(payload, dto.RazorpaySignature, keySecret);

        if (!isValid)
        {
            throw new BusinessRuleException("Payment signature verification failed. The payment could not be confirmed.");
        }

        // 4. Remove pending entry so it can't be replayed
        _memoryCache.Remove(cacheKey);

        // 5. Create the policy
        var product = await _policyRepository.GetProductByIdAsync(pending.ProductId)
                      ?? throw new NotFoundException("Insurance product not found.");

        var issuedDate    = DateTime.UtcNow;
        var insuranceDate = DateOnly.FromDateTime(pending.InsuranceDate.Date);

        var policy = new Policy
        {
            PolicyId       = Guid.NewGuid(),
            PolicyNumber   = $"SS-{issuedDate:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            UserId         = userId,
            TypeId         = product.TypeId,
            SubTypeId      = product.SubTypeId,
            IssuedDate     = issuedDate,
            InsuranceDate  = insuranceDate,
            EndDate        = insuranceDate.AddMonths(pending.TermMonths),
            CoverageAmount = pending.CoverageAmount,
            MonthlyPremium = pending.MonthlyPremium,
            Status         = PolicyStatus.Active,
            CreatedAt      = issuedDate,
            UpdatedAt      = issuedDate
        };

        await _policyRepository.AddAsync(policy);

        // 6. Record the payment
        var payment = new PolicyPayment
        {
            PaymentId      = Guid.NewGuid(),
            PolicyId       = policy.PolicyId,
            Amount         = pending.MonthlyPremium,
            PaymentDate    = issuedDate,
            PaymentMethod  = "RAZORPAY",
            TransactionRef = dto.RazorpayPaymentId,
            Status         = "SUCCESS"
        };

        await _policyRepository.AddPaymentAsync(payment);
        await _policyRepository.SaveChangesAsync();

        // 7. Invalidate user policy cache
        _memoryCache.Remove($"user_policies_{userId}");

        // 8. Publish event — non-fatal: policy is already saved, messaging failure must not roll it back
        try
        {
            await _eventPublisher.PublishActivatedAsync(new PolicyActivatedEvent
            {
                PolicyId       = policy.PolicyId,
                UserId         = policy.UserId,
                PolicyNumber   = policy.PolicyNumber,
                ActivatedAtUtc = issuedDate
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PolicyActivatedEvent publish failed for policy {PolicyId}. RabbitMQ may be down.", policy.PolicyId);
        }

        return MapPolicy(policy);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Validates all purchase input fields before creating a Razorpay order.</summary>
    private static void ValidatePurchaseInput(int productId, decimal coverageAmount, int termMonths, DateTime insuranceDate)
    {
        if (productId <= 0)
            throw new ValidationException("Please select a valid insurance product.");

        if (coverageAmount < 10_000)
            throw new ValidationException("Coverage amount must be at least ₹10,000.");

        if (coverageAmount > 50_000_000)
            throw new ValidationException("Coverage amount cannot exceed ₹5,00,00,000.");

        if (termMonths < 1 || termMonths > 120)
            throw new ValidationException("Policy term must be between 1 and 120 months.");

        if (insuranceDate.Date < DateTime.UtcNow.Date)
            throw new ValidationException("Insurance start date cannot be in the past.");

        if (insuranceDate.Date > DateTime.UtcNow.Date.AddYears(1))
            throw new ValidationException("Insurance start date cannot be more than 1 year in the future.");
    }

    /// <summary>
    /// Calculates the monthly premium using base premium, coverage factor, and term factor.
    /// Minimum term factor is 0.5 to avoid very low premiums for short-term policies.
    /// </summary>
    private static decimal CalculatePremium(decimal basePremium, decimal coverageAmount, int termMonths)
    {
        var coverageFactor = coverageAmount / 100_000m;
        var termFactor     = termMonths / 12m;
        return Math.Round(basePremium * coverageFactor * Math.Max(termFactor, 0.5m), 2);
    }

    /// <summary>
    /// Verifies the Razorpay webhook signature using HMAC-SHA256.
    /// Razorpay signs the payload as: razorpay_order_id + "|" + razorpay_payment_id.
    /// </summary>
    private static bool VerifySignature(string payload, string signature, string secret)
    {
        var keyBytes     = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac     = new HMACSHA256(keyBytes);
        var computedBytes  = hmac.ComputeHash(payloadBytes);
        var computedHex    = Convert.ToHexString(computedBytes).ToLowerInvariant();

        return string.Equals(computedHex, signature.ToLowerInvariant(), StringComparison.Ordinal);
    }

    private static PolicyDto MapPolicy(Policy policy) => new()
    {
        PolicyId       = policy.PolicyId,
        PolicyNumber   = policy.PolicyNumber,
        UserId         = policy.UserId,
        TypeId         = policy.TypeId,
        SubTypeId      = policy.SubTypeId,
        TypeName       = policy.Type?.TypeName ?? string.Empty,
        SubTypeName    = policy.SubType?.SubTypeName ?? string.Empty,
        IssuedDate     = policy.IssuedDate,
        InsuranceDate  = policy.InsuranceDate,
        CoverageAmount = policy.CoverageAmount,
        MonthlyPremium = policy.MonthlyPremium,
        EndDate        = policy.EndDate,
        Status         = policy.Status,
        CreatedAt      = policy.CreatedAt,
        UpdatedAt      = policy.UpdatedAt
    };

    // ── Nested pending-purchase record (stored in IMemoryCache) ───────────────

    private sealed class PendingPurchase
    {
        public Guid     UserId          { get; set; }
        public int      ProductId       { get; set; }
        public decimal  CoverageAmount  { get; set; }
        public int      TermMonths      { get; set; }
        public DateTime InsuranceDate   { get; set; }
        public decimal  MonthlyPremium  { get; set; }
        public long     AmountPaise     { get; set; }
        public string   RazorpayOrderId { get; set; } = string.Empty;
    }
}
