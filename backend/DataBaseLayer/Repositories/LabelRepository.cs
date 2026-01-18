using DataBaseLayer.Context;
using DataBaseLayer.Entities;
using DataBaseLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataBaseLayer.Repositories
{
    public class LabelRepository : ILabelRepository
    {
        private readonly FundooAppDbContext _context;

        public LabelRepository(FundooAppDbContext context)
        {
            _context = context;
        }

        public async Task<Label?> GetByIdAsync(int id)
        {
            return await _context.Labels
                .Include(l => l.NoteLabels)
                    .ThenInclude(nl => nl.Note)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Label>> GetByUserAsync(int userId)
        {
            return await _context.Labels
                .Where(l => l.UserId == userId)
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsForUserAsync(string name, int userId, int? excludeLabelId = null)
        {
            var query = _context.Labels
                .Where(l => l.UserId == userId && l.Name.ToLower() == name.ToLower());

            if (excludeLabelId.HasValue)
            {
                query = query.Where(l => l.Id != excludeLabelId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task AddAsync(Label label)
        {
            await _context.Labels.AddAsync(label);
        }

        public Task DeleteAsync(Label label)
        {
            _context.Labels.Remove(label);
            return Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}