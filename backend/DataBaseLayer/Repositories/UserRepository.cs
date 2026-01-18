using DataBaseLayer.Context;
using DataBaseLayer.Entities;
using DataBaseLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataBaseLayer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly FundooAppDbContext _context;

        public UserRepository(FundooAppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
        }

        public async Task<User?> GetByResetTokenAsync(string resetToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.PasswordResetToken == resetToken);
        }

        public async Task<User?> GetByEmailVerificationTokenAsync(string token)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.EmailVerificationToken == token);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            return Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}