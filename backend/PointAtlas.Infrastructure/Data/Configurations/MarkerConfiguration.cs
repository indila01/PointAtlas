using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PointAtlas.Core.Entities;

namespace PointAtlas.Infrastructure.Data.Configurations;

public class MarkerConfiguration : IEntityTypeConfiguration<Marker>
{
    public void Configure(EntityTypeBuilder<Marker> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .HasMaxLength(2000);

        builder.Property(m => m.Category)
            .IsRequired()
            .HasMaxLength(100);

        // Configure spatial column with SRID 4326 (WGS 84)
        builder.Property(m => m.Location)
            .HasColumnType("geography (point)")
            .IsRequired();

        // Create spatial index for efficient location queries
        builder.HasIndex(m => m.Location)
            .HasMethod("GIST");

        builder.Property(m => m.Latitude)
            .IsRequired();

        builder.Property(m => m.Longitude)
            .IsRequired();

        // Store Properties as JSONB
        builder.Property(m => m.Properties)
            .HasColumnType("jsonb");

        builder.Property(m => m.CreatedById)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(m => m.CreatedBy)
            .WithMany(u => u.Markers)
            .HasForeignKey(m => m.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(m => m.Category);
        builder.HasIndex(m => m.CreatedById);
        builder.HasIndex(m => m.CreatedAt);
    }
}
