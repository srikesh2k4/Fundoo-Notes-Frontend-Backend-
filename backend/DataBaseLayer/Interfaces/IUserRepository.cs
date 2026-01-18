using DataBaseLayer.Entities;

namespace DataBaseLayer.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task<User?> GetByResetTokenAsync(string resetToken);
        Task<User?> GetByEmailVerificationTokenAsync(string token);
        Task AddAsync(User user);
        Task DeleteAsync(User user);
        Task SaveAsync();
    }
}