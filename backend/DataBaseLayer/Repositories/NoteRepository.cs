using DataBaseLayer.Context;
using DataBaseLayer.Entities;
using DataBaseLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataBaseLayer.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly FundooAppDbContext _context;

        public NoteRepository(FundooAppDbContext context)
        {
            _context = context;
        }

        public async Task<Note?> GetByIdAsync(int id)
        {
            return await _context.Notes
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Include(n => n.Collaborators)
                    .ThenInclude(c => c.CollaboratorUser)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<Note>> GetByUserIdAsync(int userId)
        {
            return await _context.Notes
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetArchivedByUserIdAsync(int userId)
        {
            return await _context.Notes
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Where(n => n.UserId == userId && n.IsArchived && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetDeletedByUserIdAsync(int userId)
        {
            return await _context.Notes
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Where(n => n.UserId == userId && n.IsDeleted)
                .OrderByDescending(n => n.DeletedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetTrashedByUserIdAsync(int userId)
        {
            return await _context.Notes
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Where(n => n.UserId == userId && n.IsDeleted)
                .OrderByDescending(n => n.DeletedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> SearchAsync(string query, int userId)
        {
            return await _context.Notes
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Where(n => n.UserId == userId && !n.IsDeleted &&
                    (n.Title!.Contains(query) || n.Content!.Contains(query)))
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByLabelIdAsync(int labelId, int userId)
        {
            return await _context.Notes
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Where(n => n.UserId == userId &&
                    !n.IsDeleted &&
                    n.NoteLabels.Any(nl => nl.LabelId == labelId))
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByIdsAsync(IEnumerable<int> noteIds)
        {
            return await _context.Notes
                .Where(n => noteIds.Contains(n.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetCollaboratedNotesAsync(int userId)
        {
            return await _context.Notes
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Include(n => n.Collaborators)
                    .ThenInclude(c => c.CollaboratorUser)
                .Where(n => n.Collaborators.Any(c => c.CollaboratorId == userId) && !n.IsDeleted)
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Note note)
        {
            await _context.Notes.AddAsync(note);
        }

        public Task DeleteAsync(Note note)
        {
            _context.Notes.Remove(note);
            return Task.CompletedTask;
        }

        public async Task DeleteAllTrashedAsync(int userId)
        {
            var trashedNotes = await _context.Notes
                .Where(n => n.UserId == userId && n.IsDeleted)
                .ToListAsync();
            
            _context.Notes.RemoveRange(trashedNotes);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
