using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace gRPCNet.ServerAPI.DAL.Repositories
{
    public class TransactionLogRepository<T> : ITransactionLogRepository<T> where T : class
    {
        private readonly DbSet<T> _set;
        private readonly TransactionLogDbContext _context;

        public TransactionLogRepository(TransactionLogDbContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        public void Add(T entity)
        {
            _set.Add(entity);
            _context.SaveChanges();
        }

        public void AddNoSave(T entity)
        {
            _set.Add(entity);
        }

        public void Add(IEnumerable<T> entities)
        {
            _set.AddRange(entities);
            _context.SaveChanges();
        }

        public void BulkInsert(IEnumerable<T> entities)
        {
            _context.BulkInsert<T>(entities, options => options.IncludeGraph = true);
        }

        public IEnumerable<T> All()
        {
            return _set.ToArray();
        }

        public IQueryable<T> AllAsQueryable()
        {
            return _set;
        }

        public IQueryable<T> AsNoTracking()
        {
            return _set.AsNoTracking();
        }

        public IEnumerable<T> Where(Expression<Func<T, bool>> predicate)
        {
            return _set.Where(predicate).ToArray();
        }

        public T FirstOrDefault()
        {
            return _set.FirstOrDefault();
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _set.FirstOrDefault(predicate);
        }

        public T GetById(object id)
        {
            return _set.Find(id);
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public bool Any()
        {
            return _set.Any();
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            return _set.Any(predicate);
        }

        public void RemovePermanent(T entity)
        {
            _set.Remove(entity);
            _context.SaveChanges();
        }

        public void BulkSaveChanges()
        {
            _context.BulkSaveChanges();
        }

        public void BulkUpdate(IList<T> entities)
        {
            _context.BulkUpdate(entities);
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            return _set.Count(predicate);
        }
    }
}
