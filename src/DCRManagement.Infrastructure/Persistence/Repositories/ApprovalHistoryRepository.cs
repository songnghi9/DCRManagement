using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DCRManagement.Infrastructure.Persistence.Repositories;

public class ApprovalHistoryRepository : GenericRepository<ApprovalHistory>, IApprovalHistoryRepository
{
    public ApprovalHistoryRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ApprovalHistory>> GetByDCRAsync(int dcrId) =>
        await _dbSet.Where(h => h.DCRId == dcrId)
                    .Include(h => h.Actor)
                    .OrderByDescending(h => h.ActionDate)
                    .ToListAsync();
}