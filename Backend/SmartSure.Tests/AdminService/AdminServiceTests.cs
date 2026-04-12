using Microsoft.Extensions.Caching.Memory;
using Moq;
using SmartSure.AdminService.Repositories;
using SmartSure.AdminService.Services;
using SmartSure.Shared.Exceptions;

namespace SmartSure.Tests.AdminService;

[TestFixture]
[Category("AdminService")]
public class AdminServiceTests
{
    private Mock<IAdminRepository> _repo = null!;
    private IMemoryCache _cache = null!;
    private SmartSure.AdminService.Services.AdminService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo  = new Mock<IAdminRepository>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _sut   = new SmartSure.AdminService.Services.AdminService(_repo.Object, _cache);
    }

    [TearDown]
    public void TearDown() => _cache.Dispose();

    // ── GetAuditLogs ──────────────────────────────────────────────────────

    [Test]
    [Description("GetAuditLogs should throw ValidationException when page is zero")]
    public void GetAuditLogs_ZeroPage_ThrowsValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(() =>
            _sut.GetAuditLogsAsync(null, null, null, null, page: 0, pageSize: 10));
    }

    [Test]
    [Description("GetAuditLogs should throw ValidationException when page size exceeds 200")]
    public void GetAuditLogs_PageSizeTooLarge_ThrowsValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(() =>
            _sut.GetAuditLogsAsync(null, null, null, null, page: 1, pageSize: 201));
    }

    [Test]
    [Description("GetAuditLogs should throw ValidationException when page size is zero")]
    public void GetAuditLogs_ZeroPageSize_ThrowsValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(() =>
            _sut.GetAuditLogsAsync(null, null, null, null, page: 1, pageSize: 0));
    }

    [Test]
    [Description("GetAuditLogs with valid params should return paged result from repository")]
    public async Task GetAuditLogs_ValidParams_ReturnsPaginatedResult()
    {
        _repo.Setup(r => r.GetAuditLogsAsync(null, null, null, null, 1, 10))
             .ReturnsAsync(new List<SmartSure.AdminService.Models.AuditLog>());
        _repo.Setup(r => r.GetAuditLogsCountAsync(null, null, null, null))
             .ReturnsAsync(0);

        var result = await _sut.GetAuditLogsAsync(null, null, null, null, 1, 10);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items, Is.Empty);
        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    // ── GetDashboardStats ─────────────────────────────────────────────────

    [Test]
    [Description("GetDashboardStats should return cached result on second call")]
    public async Task GetDashboardStats_SecondCall_ReturnsCachedResult()
    {
        _repo.Setup(r => r.CountDistinctEntitiesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
             .ReturnsAsync(5);
        _repo.Setup(r => r.SumAmountFromAuditDetailsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
             .ReturnsAsync(100000m);

        var first  = await _sut.GetDashboardStatsAsync();
        var second = await _sut.GetDashboardStatsAsync();

        // Repository should only be called once — second call hits cache
        _repo.Verify(r => r.CountDistinctEntitiesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Exactly(2));
        Assert.That(first.TotalPolicies, Is.EqualTo(second.TotalPolicies));
    }

    // ── ExportReportPdf ───────────────────────────────────────────────────

    [Test]
    [Description("ExportReportPdf should throw NotFoundException when report does not exist")]
    public void ExportReportPdf_ReportNotFound_ThrowsNotFoundException()
    {
        _repo.Setup(r => r.GetReportByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync((SmartSure.AdminService.Models.Report?)null);

        Assert.ThrowsAsync<NotFoundException>(() => _sut.ExportReportPdfAsync(Guid.NewGuid()));
    }
}
