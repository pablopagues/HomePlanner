using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Framework.HomePlanner;

public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>
    where TEntity : class
{
    private readonly DbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public IQueryable<TEntity> GetAll() => _dbSet.AsQueryable();

    public IQueryable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] includeExpressions)
    {
        IQueryable<TEntity> query = _dbSet;
        foreach (var include in includeExpressions)
            query = query.Include(include);
        return query;
    }

    public IQueryable<TEntity> Get(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        params Expression<Func<TEntity, object>>[] includeExpressions)
    {
        IQueryable<TEntity> query = _dbSet;

        foreach (var include in includeExpressions)
            query = query.Include(include);

        if (filter != null)
            query = query.Where(filter);

        return orderBy != null ? orderBy(query) : query;
    }

    public TEntity? GetById(TKey id) => _dbSet.Find(id);

    public void Insert(TEntity entity) => _dbSet.Add(entity);

    public void Insert(IList<TEntity> entities)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            _dbSet.AddRange(entities);
            _context.SaveChanges();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void Delete(TKey id)
    {
        var entity = _dbSet.Find(id);
        if (entity != null) Delete(entity);
    }

    public void Delete(TEntity entity)
    {
        if (_context.Entry(entity).State == EntityState.Detached)
            _dbSet.Attach(entity);
        _dbSet.Remove(entity);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Update(IList<TEntity> entities)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            foreach (var entity in entities)
            {
                _dbSet.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            }
            _context.SaveChanges();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

}
