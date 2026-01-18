using DataBaseLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataBaseLayer.Context
{
    public class FundooAppDbContext : DbContext
    {
        public FundooAppDbContext(DbContextOptions<FundooAppDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<Note> Notes => Set<Note>();
        public DbSet<Label> Labels => Set<Label>();
        public DbSet<NoteLabel> NoteLabels => Set<NoteLabel>();
        public DbSet<Collaborator> Collaborators => Set<Collaborator>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FundooAppDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Automatically set UpdatedAt for modified entities
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Note note)
                {
                    note.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is User user)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is Collaborator collaborator)
                {
                    collaborator.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}