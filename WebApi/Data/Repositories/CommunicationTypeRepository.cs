using Microsoft.EntityFrameworkCore;
using WebApi.Data.Entities;
using WebApi.Data.Repositories.Interfaces;

namespace WebApi.Data.Repositories;

public class CommunicationTypeRepository : Repository<CommunicationType>, ICommunicationTypeRepository
{
    public CommunicationTypeRepository(CommunicationDbContext context) : base(context)
    {
    }

    public async Task<List<CommunicationType>> GetAllActiveAsync()
    {
        return await _dbSet
            .Where(ct => ct.IsActive)
            .Include(ct => ct.ValidStatuses.Where(vs => vs.IsActive))
                .ThenInclude(vs => vs.GlobalStatus)
            .OrderBy(ct => ct.DisplayName)
            .ToListAsync();
    }

    public async Task<CommunicationType?> GetByCodeWithStatusesAsync(string typeCode)
    {
        return await _dbSet
            .Include(ct => ct.ValidStatuses.Where(vs => vs.IsActive))
                .ThenInclude(vs => vs.GlobalStatus)
            .FirstOrDefaultAsync(ct => ct.TypeCode == typeCode);
    }

    public async Task<bool> HasActiveCommunicationsAsync(string typeCode)
    {
        return await _context.Communications
            .AnyAsync(c => c.TypeCode == typeCode);
    }

    public override async Task<CommunicationType?> GetByIdAsync(object id)
    {
        return await GetByCodeWithStatusesAsync(id.ToString()!);
    }
}
