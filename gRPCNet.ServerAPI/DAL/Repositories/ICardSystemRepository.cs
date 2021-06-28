using gRPCNet.ServerAPI.Models.Domain.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace gRPCNet.ServerAPI.DAL.Repositories
{
    public interface ICardSystemRepository<T> where T : class
    {
        void Add(T entity);
        void AddNoSave(T entity);
        void Add(IEnumerable<T> entities);
        void BulkInsert(IEnumerable<T> entities);
        void Update();
        T FirstOrDefault();
        T FirstOrDefault(Expression<Func<T, bool>> predicate);
        T GetById(object id);
        bool Any();
        bool Any(Expression<Func<T, bool>> predicate);
        IQueryable<T> AllAsQueryable();
        IQueryable<T> AsNoTracking();
        IEnumerable<T> All();
        IEnumerable<T> Where(Expression<Func<T, bool>> predicate);
        IdentityDbContext<User> Context { get; }

        void RemovePermanent(T entity);
        void BulkSaveChanges();
        void BulkUpdate(IList<T> entities);
        int Count(Expression<Func<T, bool>> predicate);
    }
}
