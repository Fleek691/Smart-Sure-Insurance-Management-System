using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Models;
using SmartSure.ClaimsService.Repositories;
using SmartSure.ClaimsService.Services;
using SmartSure.Shared.Constants;
using SmartSure.Shared.Exceptions;

namespace SmartSure.Tests.ClaimsService;

[TestFixture]
[Category("ClaimsService")]
public class ClaimServiceTests
{
    private Mock<IClaimRepository> _repo = null!;
    private Mock<IClaimEventPublisher> _publisher = null!;
    private Mock<IMegaStorageService> _storage = null!;
    private IMemoryCache _cache = null!;
    private SmartSure.ClaimsService.Services.ClaimService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo      = new Mock<IClaimRepository>();
        _publisher = new Mock<IClaimEventPublisher>();
        _storage   = new Mock<IMegaStorageService>();
        _cache     = new MemoryCache(new MemoryCacheOptions());

        var policyVerification = new Mock<IPolicyVerificationService>();
        // Default: return null (policy verification skipped when token is empty)
        policyVerification.Setup(p => p.GetPolicyStatusAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                          .ReturnsAsync((string?)null);

        _sut = new SmartSure.ClaimsService.Services.ClaimService(
            _repo.Object, _publisher.Object, _cache, _storage.Object, policyVerification.Object);
    }

    [TearDown]
    public void TearDown() => _cache.Dispose();

    // ── CreateClaim ───────────────────────────────────────────────────────

    [Test]
    [Description("CreateClaim should throw ValidationException when description is too short")]
    public void CreateClaim_ShortDescription_ThrowsValidationException()
    {
        var dto = new CreateClaimDto { PolicyId = Guid.NewGuid(), Description = "short", ClaimAmount = 5000 };
        Assert.ThrowsAsync<ValidationException>(() => _sut.CreateClaimAsync(Guid.NewGuid(), dto));
    }

    [Test]
    [Description("CreateClaim should throw ValidationException when claim amount is zero")]
    public void CreateClaim_ZeroAmount_ThrowsValidationException()
    {
        var dto = new CreateClaimDto { PolicyId = Guid.NewGuid(), Description = "Detailed description here", ClaimAmount = 0 };
        Assert.ThrowsAsync<ValidationException>(() => _sut.CreateClaimAsync(Guid.NewGuid(), dto));
    }

    [Test]
    [Description("CreateClaim with valid data should save and return a DRAFT claim")]
    public async Task CreateClaim_ValidData_ReturnsDraftClaim()
    {
        var userId   = Guid.NewGuid();
        var policyId = Guid.NewGuid();
        var dto      = new CreateClaimDto
        {
            PolicyId    = policyId,
            Description = "My car was damaged in an accident on the highway.",
            ClaimAmount = 50000
        };

        _repo.Setup(r => r.AddAsync(It.IsAny<Claim>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddStatusHistoryAsync(It.IsAny<ClaimStatusHistory>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.CreateClaimAsync(userId, dto);

        Assert.That(result.Status, Is.EqualTo(ClaimStatus.Draft));
        Assert.That(result.ClaimAmount, Is.EqualTo(50000));
        Assert.That(result.UserId, Is.EqualTo(userId));
    }

    // ── SubmitClaim ───────────────────────────────────────────────────────

    [Test]
    [Description("SubmitClaim should throw NotFoundException when claim does not exist")]
    public void SubmitClaim_ClaimNotFound_ThrowsNotFoundException()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Claim?)null);
        Assert.ThrowsAsync<NotFoundException>(() => _sut.SubmitClaimAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Test]
    [Description("SubmitClaim should throw ForbiddenException when user does not own the claim")]
    public void SubmitClaim_WrongUser_ThrowsForbiddenException()
    {
        var claim = new Claim { ClaimId = Guid.NewGuid(), UserId = Guid.NewGuid(), Status = ClaimStatus.Draft };
        _repo.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);

        Assert.ThrowsAsync<ForbiddenException>(() => _sut.SubmitClaimAsync(claim.ClaimId, Guid.NewGuid()));
    }

    [Test]
    [Description("SubmitClaim should throw BusinessRuleException when claim is not in DRAFT status")]
    public void SubmitClaim_NotDraft_ThrowsBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var claim  = new Claim { ClaimId = Guid.NewGuid(), UserId = userId, Status = ClaimStatus.Submitted };
        _repo.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);

        Assert.ThrowsAsync<BusinessRuleException>(() => _sut.SubmitClaimAsync(claim.ClaimId, userId));
    }

    [Test]
    [Description("SubmitClaim should throw BusinessRuleException when no documents are attached")]
    public async Task SubmitClaim_NoDocuments_ThrowsBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var claim  = new Claim { ClaimId = Guid.NewGuid(), UserId = userId, Status = ClaimStatus.Draft };
        _repo.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);
        _repo.Setup(r => r.GetDocumentsAsync(claim.ClaimId)).ReturnsAsync(new List<ClaimDocument>());

        Assert.ThrowsAsync<BusinessRuleException>(() => _sut.SubmitClaimAsync(claim.ClaimId, userId));
        await Task.CompletedTask;
    }

    [Test]
    [Description("SubmitClaim with documents should transition claim to SUBMITTED and publish events")]
    public async Task SubmitClaim_WithDocuments_TransitionsToSubmitted()
    {
        var userId = Guid.NewGuid();
        var claim  = new Claim
        {
            ClaimId     = Guid.NewGuid(),
            ClaimNumber = "CLM-20260412-1234",
            PolicyId    = Guid.NewGuid(),
            UserId      = userId,
            Status      = ClaimStatus.Draft,
            UpdatedAt   = DateTime.UtcNow
        };
        var docs = new List<ClaimDocument> { new() { DocId = Guid.NewGuid(), ClaimId = claim.ClaimId } };

        _repo.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);
        _repo.Setup(r => r.GetDocumentsAsync(claim.ClaimId)).ReturnsAsync(docs);
        _repo.Setup(r => r.AddStatusHistoryAsync(It.IsAny<ClaimStatusHistory>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _publisher.Setup(p => p.PublishClaimSubmittedAsync(It.IsAny<SmartSure.Shared.Events.ClaimSubmittedEvent>())).Returns(Task.CompletedTask);
        _publisher.Setup(p => p.PublishClaimStatusChangedAsync(It.IsAny<SmartSure.Shared.Events.ClaimStatusChangedEvent>())).Returns(Task.CompletedTask);

        var result = await _sut.SubmitClaimAsync(claim.ClaimId, userId);

        Assert.That(result.Status, Is.EqualTo(ClaimStatus.Submitted));
        _publisher.Verify(p => p.PublishClaimSubmittedAsync(It.IsAny<SmartSure.Shared.Events.ClaimSubmittedEvent>()), Times.Once);
    }

    // ── UploadDocument ────────────────────────────────────────────────────

    [Test]
    [Description("UploadDocument should throw ValidationException for unsupported file type")]
    public async Task UploadDocument_InvalidFileType_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var claim  = new Claim { ClaimId = Guid.NewGuid(), UserId = userId, Status = ClaimStatus.Draft };
        _repo.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);

        var dto = new UploadClaimDocumentDto { FileName = "doc.exe", FileType = "exe", FileSizeKb = 100, ContentBase64 = "dGVzdA==" };
        Assert.ThrowsAsync<ValidationException>(() => _sut.UploadDocumentAsync(claim.ClaimId, userId, dto));
        await Task.CompletedTask;
    }

    [Test]
    [Description("UploadDocument should throw ValidationException when file exceeds 10 MB")]
    public async Task UploadDocument_FileTooLarge_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var claim  = new Claim { ClaimId = Guid.NewGuid(), UserId = userId, Status = ClaimStatus.Draft };
        _repo.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);

        var dto = new UploadClaimDocumentDto { FileName = "big.pdf", FileType = "pdf", FileSizeKb = 15000, ContentBase64 = "dGVzdA==" };
        Assert.ThrowsAsync<ValidationException>(() => _sut.UploadDocumentAsync(claim.ClaimId, userId, dto));
        await Task.CompletedTask;
    }

    [Test]
    [Description("UploadDocument should throw BusinessRuleException when claim is not in DRAFT status")]
    public async Task UploadDocument_NonDraftClaim_ThrowsBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var claim  = new Claim { ClaimId = Guid.NewGuid(), UserId = userId, Status = ClaimStatus.Submitted };
        _repo.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);

        var dto = new UploadClaimDocumentDto { FileName = "doc.pdf", FileType = "pdf", FileSizeKb = 100, ContentBase64 = "dGVzdA==" };
        Assert.ThrowsAsync<BusinessRuleException>(() => _sut.UploadDocumentAsync(claim.ClaimId, userId, dto));
        await Task.CompletedTask;
    }
}
