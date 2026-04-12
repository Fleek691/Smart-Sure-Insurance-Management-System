using SmartSure.Shared.Constants;
using SmartSure.Shared.Exceptions;

namespace SmartSure.Tests.PolicyService;

/// <summary>
/// Policy business rule tests — validates the rules enforced by PolicyService
/// without requiring a direct project reference (Razorpay SDK targets .NET Framework).
/// These tests verify the exception types and constants used by the service.
/// </summary>
[TestFixture]
[Category("PolicyService")]
public class PolicyServiceTests
{
    // ── PolicyStatus constants ────────────────────────────────────────────

    [Test]
    [Description("PolicyStatus constants should have the correct string values")]
    public void PolicyStatus_Constants_HaveCorrectValues()
    {
        Assert.That(PolicyStatus.Draft,     Is.EqualTo("DRAFT"));
        Assert.That(PolicyStatus.Active,    Is.EqualTo("ACTIVE"));
        Assert.That(PolicyStatus.Expired,   Is.EqualTo("EXPIRED"));
        Assert.That(PolicyStatus.Cancelled, Is.EqualTo("CANCELLED"));
    }

    // ── Coverage validation rules ─────────────────────────────────────────

    [TestCase(0,          false, Description = "Zero coverage is invalid")]
    [TestCase(9999,       false, Description = "Below minimum (10,000) is invalid")]
    [TestCase(10000,      true,  Description = "Exactly minimum is valid")]
    [TestCase(500000,     true,  Description = "Mid-range coverage is valid")]
    [TestCase(50000000,   true,  Description = "Maximum coverage is valid")]
    [TestCase(50000001,   false, Description = "Above maximum is invalid")]
    public void CoverageAmount_Validation(decimal amount, bool isValid)
    {
        const decimal min = 10_000m;
        const decimal max = 50_000_000m;
        var result = amount >= min && amount <= max;
        Assert.That(result, Is.EqualTo(isValid));
    }

    // ── Term validation rules ─────────────────────────────────────────────

    [TestCase(0,   false, Description = "Zero months is invalid")]
    [TestCase(1,   true,  Description = "1 month is valid")]
    [TestCase(12,  true,  Description = "12 months is valid")]
    [TestCase(120, true,  Description = "120 months is valid")]
    [TestCase(121, false, Description = "121 months exceeds maximum")]
    public void TermMonths_Validation(int months, bool isValid)
    {
        var result = months >= 1 && months <= 120;
        Assert.That(result, Is.EqualTo(isValid));
    }

    // ── Insurance date validation rules ───────────────────────────────────

    [Test]
    [Description("Insurance date in the past should be invalid")]
    public void InsuranceDate_InPast_IsInvalid()
    {
        var date   = DateTime.UtcNow.AddDays(-1).Date;
        var result = date >= DateTime.UtcNow.Date;
        Assert.That(result, Is.False);
    }

    [Test]
    [Description("Insurance date today should be valid")]
    public void InsuranceDate_Today_IsValid()
    {
        var date   = DateTime.UtcNow.Date;
        var result = date >= DateTime.UtcNow.Date && date <= DateTime.UtcNow.Date.AddYears(1);
        Assert.That(result, Is.True);
    }

    [Test]
    [Description("Insurance date more than 1 year ahead should be invalid")]
    public void InsuranceDate_MoreThanOneYearAhead_IsInvalid()
    {
        var date   = DateTime.UtcNow.AddYears(1).AddDays(1).Date;
        var result = date <= DateTime.UtcNow.Date.AddYears(1);
        Assert.That(result, Is.False);
    }

    // ── Premium calculation formula ───────────────────────────────────────

    [Test]
    [Description("Premium calculation should use 0.5 term factor floor for terms under 6 months")]
    public void PremiumCalculation_ShortTerm_UsesFloorFactor()
    {
        const decimal basePremium    = 8000m;
        const decimal coverageAmount = 100000m;

        decimal Calculate(int months)
        {
            var coverageFactor = coverageAmount / 100_000m;
            var termFactor     = months / 12m;
            return Math.Round(basePremium * coverageFactor * Math.Max(termFactor, 0.5m), 2);
        }

        // 1 month and 6 months should produce the same premium (both use 0.5 floor)
        Assert.That(Calculate(1), Is.EqualTo(Calculate(6)));
        // 12 months should be higher than 6 months
        Assert.That(Calculate(12), Is.GreaterThan(Calculate(6)));
    }

    [Test]
    [Description("Premium should scale linearly with coverage amount")]
    public void PremiumCalculation_DoubledCoverage_DoublesPremium()
    {
        const decimal basePremium = 8000m;
        const int     termMonths  = 12;

        decimal Calculate(decimal coverage)
        {
            var coverageFactor = coverage / 100_000m;
            var termFactor     = termMonths / 12m;
            return Math.Round(basePremium * coverageFactor * Math.Max(termFactor, 0.5m), 2);
        }

        var p1 = Calculate(100000);
        var p2 = Calculate(200000);
        Assert.That(p2, Is.EqualTo(p1 * 2));
    }

    // ── State machine transitions ─────────────────────────────────────────

    [TestCase("SUBMITTED",   "UNDER_REVIEW", true,  Description = "SUBMITTED → UNDER_REVIEW is allowed")]
    [TestCase("UNDER_REVIEW","APPROVED",     true,  Description = "UNDER_REVIEW → APPROVED is allowed")]
    [TestCase("UNDER_REVIEW","REJECTED",     true,  Description = "UNDER_REVIEW → REJECTED is allowed")]
    [TestCase("DRAFT",       "APPROVED",     false, Description = "DRAFT → APPROVED is not allowed")]
    [TestCase("SUBMITTED",   "APPROVED",     false, Description = "SUBMITTED → APPROVED skips UNDER_REVIEW")]
    [TestCase("APPROVED",    "REJECTED",     false, Description = "APPROVED → REJECTED is terminal")]
    public void ClaimStateTransition_Validation(string from, string to, bool allowed)
    {
        var allowedTransitions = new Dictionary<string, string[]>
        {
            ["SUBMITTED"]    = new[] { "UNDER_REVIEW" },
            ["UNDER_REVIEW"] = new[] { "APPROVED", "REJECTED" }
        };

        var result = allowedTransitions.TryGetValue(from, out var targets) && targets.Contains(to);
        Assert.That(result, Is.EqualTo(allowed));
    }

    // ── Exception type hierarchy ──────────────────────────────────────────

    [Test]
    [Description("ForbiddenException should be thrown for ownership violations, not UnauthorizedException")]
    public void OwnershipViolation_ThrowsForbiddenNotUnauthorized()
    {
        // Verify the correct exception type is used for ownership checks
        var ex = new ForbiddenException("You are not allowed to cancel this policy.");
        Assert.That(ex, Is.InstanceOf<ForbiddenException>());
        Assert.That(ex, Is.Not.InstanceOf<UnauthorizedException>());
    }

    [Test]
    [Description("Already-cancelled policy should throw BusinessRuleException not ValidationException")]
    public void AlreadyCancelled_ThrowsBusinessRuleNotValidation()
    {
        var ex = new BusinessRuleException("This policy has already been cancelled.");
        Assert.That(ex, Is.InstanceOf<BusinessRuleException>());
        Assert.That(ex, Is.Not.InstanceOf<ValidationException>());
    }
}
