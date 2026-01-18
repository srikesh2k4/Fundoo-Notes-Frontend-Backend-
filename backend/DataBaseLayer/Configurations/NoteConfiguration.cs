using DataBaseLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBaseLayer.Configurations
{
    public class NoteConfiguration : IEntityTypeConfiguration<Note>
    {
        public void Configure(EntityTypeBuilder<Note> builder)
        {
            builder.ToTable("Notes");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(200);

            builder.Property(x => x.Content)
                .HasMaxLength(10000);

            builder.Property(x => x.Color)
                .HasMaxLength(7)
                .HasDefaultValue("#FFFFFF");

            builder.Property(x => x.IsPinned)
                .HasDefaultValue(false);

            builder.Property(x => x.IsArchived)
                .HasDefaultValue(false);

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_Notes_UserId");

            builder.HasIndex(x => new { x.UserId, x.IsDeleted })
                .HasDatabaseName("IX_Notes_UserId_IsDeleted");

            builder.HasIndex(x => new { x.UserId, x.IsPinned })
                .HasDatabaseName("IX_Notes_UserId_IsPinned");

            // Relationships
            builder.HasMany(n => n.NoteLabels)
                .WithOne(nl => nl.Note)
                .HasForeignKey(nl => nl.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(n => n.Collaborators)
                .WithOne(c => c.Note)
                .HasForeignKey(c => c.NoteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}