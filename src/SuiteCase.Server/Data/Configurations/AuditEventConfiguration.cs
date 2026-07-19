using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data.Configurations;

public sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents");

        builder.HasKey(auditEvent => auditEvent.Id);

        builder.Property(auditEvent => auditEvent.OperationId)
            .IsRequired();

        builder.Property(auditEvent => auditEvent.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(auditEvent => auditEvent.EntityType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(auditEvent => auditEvent.EntityId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(auditEvent => auditEvent.ActorId)
            .HasMaxLength(450);

        builder.Property(auditEvent => auditEvent.CorrelationId)
            .HasMaxLength(100);

        builder.Property(auditEvent => auditEvent.Details)
            .HasMaxLength(2000);

        builder.Property(auditEvent => auditEvent.OccurredAt)
            .IsRequired();

        builder.HasIndex(auditEvent => new
        {
            auditEvent.EntityType,
            auditEvent.EntityId,
            auditEvent.OccurredAt
        });
        builder.HasIndex(auditEvent => auditEvent.OccurredAt);
        builder.HasIndex(auditEvent => auditEvent.OperationId).IsUnique();
        builder.HasIndex(auditEvent => auditEvent.Action);
        builder.HasIndex(auditEvent => auditEvent.ActorId);
        builder.HasIndex(auditEvent => auditEvent.CorrelationId);
    }
}
