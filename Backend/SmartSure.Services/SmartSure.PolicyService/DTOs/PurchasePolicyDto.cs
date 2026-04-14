using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs;

/// <summary>Request body for the direct (non-Razorpay) policy purchase endpoint.</summary>
public class PurchasePolicyDto
{
    [Required]
    public int ProductId { get; set; }

    /// <summary>Desired coverage amount in INR — must be at least ₹10,000.</summary>
    [Range(1, double.MaxValue)]
    public decimal CoverageAmount { get; set; }

    /// <summary>Policy term in months — between 1 and 120.</summary>
    [Range(1, 120)]
    public int TermMonths { get; set; }

    /// <summary>Date from which coverage should begin.</summary>
    public DateTime InsuranceDate { get; set; }
}