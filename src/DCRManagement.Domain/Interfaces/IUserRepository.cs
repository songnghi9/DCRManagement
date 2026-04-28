using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Enums;

namespace DCRManagement.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
    Task<bool> ValidateCredentialsAsync(string username, string passwordHash);
}