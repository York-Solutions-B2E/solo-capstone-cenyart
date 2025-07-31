
using Microsoft.EntityFrameworkCore;
using WebApi.Data.Repositories.Interfaces;

namespace WebApi.Data.Repositories;

public class Repository<T>(CommunicationDbContext context) : IRepository<T> where T : class
{
    protected readonly CommunicationDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(object id)
    {
        var entity = await _dbSet.FindAsync(id);
        return entity != null;
    }
}
