using DataBaseLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBaseLayer.Configurations
{
    public class NoteLabelConfiguration : IEntityTypeConfiguration<NoteLabel>
    {
        public void Configure(EntityTypeBuilder<NoteLabel> builder)
        {
            builder.ToTable("NoteLabels");

            // Composite Primary Key
            builder.HasKey(nl => new { nl.NoteId, nl.LabelId });

            // Properties
            builder.Property(nl => nl.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(nl => nl.Note)
                .WithMany(n => n.NoteLabels)
                .HasForeignKey(nl => nl.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(nl => nl.Label)
                .WithMany(l => l.NoteLabels)
                .HasForeignKey(nl => nl.LabelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(nl => nl.NoteId)
                .HasDatabaseName("IX_NoteLabels_NoteId");

            builder.HasIndex(nl => nl.LabelId)
                .HasDatabaseName("IX_NoteLabels_LabelId");
        }
    }
}