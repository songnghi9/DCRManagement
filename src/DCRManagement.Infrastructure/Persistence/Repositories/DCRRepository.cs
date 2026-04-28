using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Enums;
using DCRManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DCRManagement.Infrastructure.Persistence.Repositories;

public class DCRRepository : GenericRepository<DCR>, IDCRRepository
{
    public DCRRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<DCR>> GetByStatusAsync(DCRStatus status) =>
        await _dbSet.Where(d => d.Status == status)
                    .Include(d => d.AssignedReviewer)
                    .Include(d => d.AssignedApprover)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();

    public async Task<IEnumerable<DCR>> GetByCreatorAsync(int userId) =>
        await _dbSet.Where(d => d.CreatedById == userId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();

    public async Task<IEnumerable<DCR>> GetAssignedToUserAsync(int userId) =>
        await _dbSet.Where(d => d.AssignedReviewerId == userId
                             || d.AssignedApproverId == userId)
                    .Include(d => d.AssignedReviewer)
                    .Include(d => d.AssignedApprover)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();

    public async Task<DCR?> GetWithFullDetailsAsync(int dcrId) =>
        await _dbSet
            .Include(d => d.AssignedReviewer)
            .Include(d => d.AssignedApprover)
            .Include(d => d.ApprovalHistories.OrderByDescending(h => h.ActionDate))
                .ThenInclude(h => h.Actor)
            .Include(d => d.Attachments)
            .FirstOrDefaultAsync(d => d.Id == dcrId);

    /// <summary>
    /// Generates sequential DCR number in format DCR-YYYY-NNNN.
    /// Uses MAX query to avoid race conditions with concurrent inserts.
    /// </summary>
    public async Task<string> GenerateNextDCRNumberAsync()
    {
        var year = DateTime.Now.Year;
        var prefix = $"DCR-{year}-";

        var lastNumber = await _dbSet
            .Where(d => d.DCRNumber.StartsWith(prefix))
            .Select(d => d.DCRNumber)
            .OrderByDescending(n => n)
            .FirstOrDefaultAsync();

        var nextSeq = lastNumber is null
            ? 1
            : int.Parse(lastNumber.Split('-')[2]) + 1;

        return $"{prefix}{nextSeq:D4}";
    }
}