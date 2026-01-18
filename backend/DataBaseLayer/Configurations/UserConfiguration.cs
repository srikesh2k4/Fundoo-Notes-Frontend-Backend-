using DataBaseLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBaseLayer.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.IsEmailVerified)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.FailedLoginAttempts)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.Property(x => x.LastLoginAt)
                .IsRequired(false);

            builder.Property(x => x.LockoutEnd)
                .IsRequired(false);

            builder.Property(x => x.EmailVerificationToken)
                .HasMaxLength(100);

            builder.Property(x => x.EmailVerificationExpiry)
                .IsRequired(false);

            builder.Property(x => x.PasswordResetToken)
                .HasMaxLength(100);

            builder.Property(x => x.PasswordResetExpiry)
                .IsRequired(false);

            builder.Property(x => x.RefreshToken)
                .HasMaxLength(500);

            builder.Property(x => x.RefreshTokenExpiry)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(x => x.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(x => x.RefreshToken)
                .HasDatabaseName("IX_Users_RefreshToken");

            builder.HasIndex(x => x.PasswordResetToken)
                .HasDatabaseName("IX_Users_PasswordResetToken");

            builder.HasIndex(x => x.EmailVerificationToken)
                .HasDatabaseName("IX_Users_EmailVerificationToken");

            // Relationships
            builder.HasMany(x => x.Notes)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Labels)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.CollaboratedNotes)
                .WithOne(c => c.CollaboratorUser)
                .HasForeignKey(c => c.CollaboratorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
