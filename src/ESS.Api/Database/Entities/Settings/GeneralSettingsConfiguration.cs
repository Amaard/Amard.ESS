using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESS.Api.Database.Entities.Settings;

public sealed class GeneralSettingsConfiguration: IEntityTypeConfiguration<GeneralSettings>
{
    public void Configure(EntityTypeBuilder<GeneralSettings> builder)
    {
        builder.ToTable("general_settings");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).IsRequired();
        builder.Property(h => h.Id).HasMaxLength(500);

        builder.Property(h => h.Key)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(100);

        builder.Property(h => h.Value)
               .HasMaxLength(2000);

        builder.Property(h => h.CreatedAt)
               .IsRequired();

        builder.Property(h => h.ModifiedAt);

        builder.HasIndex(h => h.Key)
               .IsUnique();
    }
}
