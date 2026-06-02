using System.Linq.Expressions;

namespace Framework.HomePlanner;

public interface IGenericRepository<TEntity, in TKey> where TEntity : class
{
    IQueryable<TEntity> GetAll();
    IQueryable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] includeExpressions);

    IQueryable<TEntity> Get(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        params Expression<Func<TEntity, object>>[] includeExpressions);

    TEntity? GetById(TKey id);

    void Insert(TEntity entity);
    void Insert(IList<TEntity> entities);

    void Delete(TKey id);
    void Delete(TEntity entity);

    void Update(TEntity entity);
    void Update(IList<TEntity> entities);
}
