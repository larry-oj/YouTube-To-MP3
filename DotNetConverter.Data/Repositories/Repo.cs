using DotNetConverter.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetConverter.Data.Repositories;

public class Repo<T> : IDisposable, IRepo<T> where T : class, IStringIdEntity
{
    private readonly ConverterDbContext _context;
    private readonly DbSet<T> _entities;

    public Repo(IDbContextFactory<ConverterDbContext> contextFactory)
    {
        _context = contextFactory.CreateDbContext();
        _entities = _context.Set<T>();
    }

    public IEnumerable<T> GetAll()
    {
        return _entities.AsEnumerable();
    }

    public T? Get(string id)
    {
        return _entities.FirstOrDefault(e => e.Id == id);
    }

    public void Insert(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _entities.Add(entity);
    }

    public void Update(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _entities.Update(entity);
    }

    public void Delete(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _entities.Remove(entity);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}