using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Enums;

namespace DCRManagement.Domain.Interfaces;

public interface IDCRRepository : IRepository<DCR>
{
    Task<IEnumerable<DCR>> GetByStatusAsync(DCRStatus status);
    Task<IEnumerable<DCR>> GetByCreatorAsync(int userId);
    Task<IEnumerable<DCR>> GetAssignedToUserAsync(int userId);
    Task<DCR?> GetWithFullDetailsAsync(int dcrId);
    Task<string> GenerateNextDCRNumberAsync();
}