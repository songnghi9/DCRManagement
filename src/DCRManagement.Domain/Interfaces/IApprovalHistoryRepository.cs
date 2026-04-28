using DCRManagement.Domain.Entities;

namespace DCRManagement.Domain.Interfaces;

public interface IApprovalHistoryRepository : IRepository<ApprovalHistory>
{
    Task<IEnumerable<ApprovalHistory>> GetByDCRAsync(int dcrId);
}