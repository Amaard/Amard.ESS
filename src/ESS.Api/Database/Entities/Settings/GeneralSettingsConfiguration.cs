﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESS.Api.Database.Entities.Settings;

public sealed class GeneralSettingsConfiguration: IEntityTypeConfiguration<GeneralSettings>
{
    public void Configure(EntityTypeBuilder<GeneralSettings> builder)
    {
        builder.ToTable("general_settings");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).IsRequired();
        builder.Property(s => s.Id).HasMaxLength(500);

        builder.Property(s => s.Key)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(s => s.Value)
               .HasMaxLength(2000);

        builder.Property(s => s.Description).HasMaxLength(500);

        builder.Property(s => s.CreatedAt)
               .IsRequired();

        builder.Property(s => s.ModifiedAt);

        builder.HasIndex(s => s.Key)
               .IsUnique();
    }
}
