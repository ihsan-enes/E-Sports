using ESpor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESpor.Infrastructure.Persistence.Configurations;

public class SyncStateConfiguration : IEntityTypeConfiguration<SyncState>
{
    public void Configure(EntityTypeBuilder<SyncState> builder)
    {
        builder.HasKey(s => s.JobName);

        builder.Property(s => s.JobName).HasMaxLength(100);
        builder.Property(s => s.LastSuccessUtc).HasColumnType("timestamptz");
        builder.Property(s => s.LastAttemptUtc).HasColumnType("timestamptz");
        builder.Property(s => s.LastError).HasMaxLength(2000);
    }
}
