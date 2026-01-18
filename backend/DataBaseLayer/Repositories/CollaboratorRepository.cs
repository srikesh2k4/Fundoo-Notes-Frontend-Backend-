using DataBaseLayer.Context;
using DataBaseLayer.Entities;
using DataBaseLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataBaseLayer.Repositories
{
    public class CollaboratorRepository : ICollaboratorRepository
    {
        private readonly FundooAppDbContext _context;

        public CollaboratorRepository(FundooAppDbContext context)
        {
            _context = context;
        }

        public async Task<Collaborator?> GetByIdAsync(int id)
        {
            return await _context.Collaborators
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Collaborator?> GetByIdWithUserAsync(int id)
        {
            return await _context.Collaborators
                .Include(c => c.CollaboratorUser)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Collaborator?> GetByNoteAndUserAsync(int noteId, int userId)
        {
            return await _context.Collaborators
                .FirstOrDefaultAsync(c => c.NoteId == noteId && c.CollaboratorId == userId);
        }

        public async Task<IEnumerable<Collaborator>> GetByNoteIdAsync(int noteId)
        {
            return await _context.Collaborators
                .Include(c => c.CollaboratorUser)
                .Where(c => c.NoteId == noteId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int noteId, int collaboratorUserId)
        {
            return await _context.Collaborators
                .AnyAsync(c => c.NoteId == noteId && c.CollaboratorId == collaboratorUserId);
        }

        public async Task AddAsync(Collaborator collaborator)
        {
            await _context.Collaborators.AddAsync(collaborator);
        }

        public Task DeleteAsync(Collaborator collaborator)
        {
            _context.Collaborators.Remove(collaborator);
            return Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}