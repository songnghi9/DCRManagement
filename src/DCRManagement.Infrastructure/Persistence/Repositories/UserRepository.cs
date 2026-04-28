using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Enums;
using DCRManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DCRManagement.Infrastructure.Persistence.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role) =>
        await _dbSet.Where(u => u.Role == role && u.IsActive).ToListAsync();

    public async Task<bool> ValidateCredentialsAsync(string username, string passwordHash) =>
        await _dbSet.AnyAsync(u => u.Username == username
                                && u.PasswordHash == passwordHash
                                && u.IsActive);
}