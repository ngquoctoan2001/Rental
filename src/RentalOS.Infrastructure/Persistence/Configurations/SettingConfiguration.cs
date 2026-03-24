using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> b)
    {
        b.ToTable("settings");
        b.HasKey(s => s.Id);
        b.HasIndex(s => s.Key).IsUnique();
        b.Property(s => s.Key).HasMaxLength(100).IsRequired();
        b.Property(s => s.Value).HasColumnType("jsonb").IsRequired();
        b.Property(s => s.UpdatedAt).HasDefaultValueSql("NOW()");
    }
}

public class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
{
    public void Configure(EntityTypeBuilder<AiConversation> b)
    {
        b.ToTable("ai_conversations");
        b.HasKey(a => a.Id);
        b.Property(a => a.Title).HasMaxLength(200);
        b.Property(a => a.Messages).HasColumnType("jsonb").HasDefaultValueSql("'[]'");
        b.Property(a => a.MessageCount).HasDefaultValue(0);

        b.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId);
        b.HasIndex(a => a.UserId);
    }
}
