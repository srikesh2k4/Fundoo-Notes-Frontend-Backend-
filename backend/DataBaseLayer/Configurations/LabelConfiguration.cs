using DataBaseLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBaseLayer.Configurations
{
    public class LabelConfiguration : IEntityTypeConfiguration<Label>
    {
        public void Configure(EntityTypeBuilder<Label> builder)
        {
            builder.ToTable("Labels");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => new { x.UserId, x.Name })
                .IsUnique()
                .HasDatabaseName("IX_Labels_UserId_Name");

            // Relationships
            builder.HasMany(l => l.NoteLabels)
                .WithOne(nl => nl.Label)
                .HasForeignKey(nl => nl.LabelId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}