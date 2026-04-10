using MassTransit;
using SmartSure.AdminService.Data;
using SmartSure.AdminService.Models;
using SmartSure.Shared.Events;

namespace SmartSure.AdminService.Consumers;

public class AuditEventConsumer(
    AdminDbContext dbContext,
    ILogger<AuditEventConsumer> logger) :
    IConsumer<UserRegisteredEvent>,
    IConsumer<PolicyActivatedEvent>,
    IConsumer<PolicyCancelledEvent>,
    IConsumer<ClaimSubmittedEvent>,
    IConsumer<ClaimStatusChangedEvent>,
    IConsumer<ClaimApprovedEvent>,
    IConsumer<ClaimRejectedEvent>
{
    private readonly AdminDbContext _dbContext = dbContext;
    private readonly ILogger<AuditEventConsumer> _logger = logger;

    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        return SaveAuditLogAsync(
            action: "USER_REGISTERED",
            entityType: "User",
            entityId: context.Message.UserId.ToString(),
            userId: context.Message.UserId,
            details: context.Message,
            timestamp: context.Message.RegisteredAtUtc,
            cancellationToken: context.CancellationToken);
    }

    public Task Consume(ConsumeContext<PolicyActivatedEvent> context)
    {
        return SaveAuditLogAsync(
            action: "POLICY_ACTIVATED",
            entityType: "Policy",
            entityId: context.Message.PolicyId.ToString(),
            userId: context.Message.UserId,
            details: context.Message,
            timestamp: context.Message.ActivatedAtUtc,
            cancellationToken: context.CancellationToken);
    }

    public Task Consume(ConsumeContext<PolicyCancelledEvent> context)
    {
        return SaveAuditLogAsync(
            action: "POLICY_CANCELLED",
            entityType: "Policy",
            entityId: context.Message.PolicyId.ToString(),
            userId: context.Message.UserId,
            details: context.Message,
            timestamp: context.Message.CancelledAtUtc,
            cancellationToken: context.CancellationToken);
    }

    public Task Consume(ConsumeContext<ClaimSubmittedEvent> context)
    {
        return SaveAuditLogAsync(
            action: "CLAIM_SUBMITTED",
            entityType: "Claim",
            entityId: context.Message.ClaimId.ToString(),
            userId: context.Message.UserId,
            details: context.Message,
            timestamp: context.Message.SubmittedAtUtc,
            cancellationToken: context.CancellationToken);
    }

    public Task Consume(ConsumeContext<ClaimStatusChangedEvent> context)
    {
        return SaveAuditLogAsync(
            action: "CLAIM_STATUS_CHANGED",
            entityType: "Claim",
            entityId: context.Message.ClaimId.ToString(),
            userId: context.Message.UserId,
            details: context.Message,
            timestamp: context.Message.ChangedAtUtc,
            cancellationToken: context.CancellationToken);
    }

    public Task Consume(ConsumeContext<ClaimApprovedEvent> context)
    {
        return SaveAuditLogAsync(
            action: "CLAIM_APPROVED",
            entityType: "Claim",
            entityId: context.Message.ClaimId.ToString(),
            userId: context.Message.UserId,
            details: context.Message,
            timestamp: context.Message.ApprovedAtUtc,
            cancellationToken: context.CancellationToken);
    }

    public Task Consume(ConsumeContext<ClaimRejectedEvent> context)
    {
        return SaveAuditLogAsync(
            action: "CLAIM_REJECTED",
            entityType: "Claim",
            entityId: context.Message.ClaimId.ToString(),
            userId: context.Message.UserId,
            details: context.Message,
            timestamp: context.Message.RejectedAtUtc,
            cancellationToken: context.CancellationToken);
    }

    private async Task SaveAuditLogAsync<T>(
        string action,
        string entityType,
        string entityId,
        Guid userId,
        T details,
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        var log = new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = System.Text.Json.JsonSerializer.Serialize(details),
            TimeStamp = timestamp == default ? DateTime.UtcNow : timestamp
        };

        _dbContext.AuditLogs.Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Audit log created for action {Action} and entity {EntityType}:{EntityId}", action, entityType, entityId);
    }
}
