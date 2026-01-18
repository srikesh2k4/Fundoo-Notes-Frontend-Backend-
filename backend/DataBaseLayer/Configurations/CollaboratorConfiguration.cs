using DataBaseLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBaseLayer.Configurations
{
    public class CollaboratorConfiguration : IEntityTypeConfiguration<Collaborator>
    {
        public void Configure(EntityTypeBuilder<Collaborator> builder)
        {
            builder.ToTable("Collaborators");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Permission)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => new { x.NoteId, x.CollaboratorId })
                .IsUnique()
                .HasDatabaseName("IX_Collaborators_NoteId_CollaboratorId");

            builder.HasIndex(x => x.NoteId)
                .HasDatabaseName("IX_Collaborators_NoteId");

            builder.HasIndex(x => x.CollaboratorId)
                .HasDatabaseName("IX_Collaborators_CollaboratorId");

            // Relationships
            builder.HasOne(c => c.Note)
                .WithMany(n => n.Collaborators)
                .HasForeignKey(c => c.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.CollaboratorUser)
                .WithMany(u => u.CollaboratedNotes)
                .HasForeignKey(c => c.CollaboratorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}