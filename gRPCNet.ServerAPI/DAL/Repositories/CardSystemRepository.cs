using gRPCNet.ServerAPI.Models.Domain.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace gRPCNet.ServerAPI.DAL.Repositories
{
    public class CardSystemRepository<T> : ICardSystemRepository<T> where T : class
    {
        private readonly DbSet<T> _set;
        private readonly CardSystemDbContext _context;

        public CardSystemRepository(CardSystemDbContext context)
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

        public IEnumerable<T> All() => _set.ToArray();

        public IQueryable<T> AllAsQueryable() => _set;

        public IQueryable<T> AsNoTracking() => _set.AsNoTracking();

        public IEnumerable<T> Where(Expression<Func<T, bool>> predicate) => _set.Where(predicate).ToArray();

        public T FirstOrDefault() => _set.FirstOrDefault();

        public T FirstOrDefault(Expression<Func<T, bool>> predicate) => _set.FirstOrDefault(predicate);

        public T GetById(object id) => _set.Find(id);

        public void Update()
        {
            _context.SaveChanges();
        }

        public bool Any() => _set.Any();

        public bool Any(Expression<Func<T, bool>> predicate) => _set.Any(predicate);

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

        public int Count(Expression<Func<T, bool>> predicate) => _set.Count(predicate);

        public IdentityDbContext<User> Context => _context;
    }
}
