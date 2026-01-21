using DataBaseLayer.Context;
using DataBaseLayer.Entities;
using DataBaseLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataBaseLayer.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly FundooAppDbContext _context; // ✅ FIXED: Changed to FundooAppDbContext

        public NoteRepository(FundooAppDbContext context) // ✅ FIXED: Changed to FundooAppDbContext
        {
            _context = context;
        }

        public async Task<Note?> GetByIdAsync(int noteId)
        {
            return await _context
                .Notes.Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Include(n => n.Collaborators)
                .FirstOrDefaultAsync(n => n.Id == noteId);
        }

        public async Task<IEnumerable<Note>> GetByUserIdAsync(int userId)
        {
            return await _context
                .Notes.Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.UpdatedAt ?? n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetByIdsAsync(IEnumerable<int> noteIds)
        {
            return await _context
                .Notes.Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Where(n => noteIds.Contains(n.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> SearchAsync(string query, int userId)
        {
            return await _context
                .Notes.Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Where(n =>
                    n.UserId == userId
                    && !n.IsDeleted
                    && (n.Title!.Contains(query) || n.Content!.Contains(query))
                )
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetTrashedByUserIdAsync(int userId)
        {
            return await _context
                .Notes.Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Where(n => n.UserId == userId && n.IsDeleted)
                .OrderByDescending(n => n.DeletedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Note note)
        {
            await _context.Notes.AddAsync(note);
        }

        public async Task DeleteAsync(Note note)
        {
            _context.Notes.Remove(note);
        }

        public async Task DeleteAllTrashedAsync(int userId)
        {
            var trashedNotes = await _context
                .Notes.Where(n => n.UserId == userId && n.IsDeleted)
                .ToListAsync();

            _context.Notes.RemoveRange(trashedNotes);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Note> AddLabelToNoteAsync(int noteId, int labelId, int userId)
        {
            // Get note with labels
            var note = await _context
                .Notes.Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);

            if (note == null)
            {
                throw new KeyNotFoundException($"Note with ID {noteId} not found");
            }

            // Check if label exists and belongs to user
            var label = await _context.Labels.FirstOrDefaultAsync(l =>
                l.Id == labelId && l.UserId == userId
            );

            if (label == null)
            {
                throw new KeyNotFoundException($"Label with ID {labelId} not found");
            }

            // Check if label is already attached
            var existingNoteLabel = note.NoteLabels.FirstOrDefault(nl => nl.LabelId == labelId);

            if (existingNoteLabel != null)
            {
                // Already attached, return current note
                return note;
            }

            // Add label to note
            var noteLabel = new NoteLabel
            {
                NoteId = noteId,
                LabelId = labelId,
                CreatedAt = DateTime.UtcNow,
            };

            note.NoteLabels.Add(noteLabel);
            note.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Reload to ensure all navigation properties are loaded
            await _context.Entry(note).ReloadAsync();
            await _context.Entry(note).Collection(n => n.NoteLabels).LoadAsync();

            return note;
        }

        public async Task<bool> RemoveLabelFromNoteAsync(int noteId, int labelId, int userId)
        {
            // Verify note belongs to user
            var note = await _context
                .Notes.Include(n => n.NoteLabels)
                .FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);

            if (note == null)
            {
                throw new KeyNotFoundException($"Note with ID {noteId} not found");
            }

            // Find the NoteLabel relationship
            var noteLabel = note.NoteLabels.FirstOrDefault(nl => nl.LabelId == labelId);

            if (noteLabel == null)
            {
                return false; // Label was not attached
            }

            note.NoteLabels.Remove(noteLabel);
            note.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
