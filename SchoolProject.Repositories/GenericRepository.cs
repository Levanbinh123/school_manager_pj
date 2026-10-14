using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SchoolProject.Repositories
{
    internal class GenericRepository<T> : IGenericRepository<T>
        where T : class
    {
        private readonly ApplicationDbContext _context;

        internal DbSet<T> dbSet;

        private bool disposed = false;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            dbSet = _context.Set<T>();
        }

        public IEnumerable<T> GetAll(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            bool disabledTracking = true)
        {
            IQueryable<T> query = dbSet;

            // Không tracking nếu chỉ đọc dữ liệu
            if (disabledTracking)
            {
                query = query.AsNoTracking();
            }

            // Filter
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Include
            if (include != null)
            {
                query = include(query);
            }

            // Order By
            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }

            return query.ToList();
        }

        public T GetById(object id)
        {
            return dbSet.Find(id);
        }

        public async Task<T> GetByIdAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            bool disabledTracking = true)
        {
            IQueryable<T> query = dbSet;

            // Không tracking
            if (disabledTracking)
            {
                query = query.AsNoTracking();
            }

            // Filter
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Include
            if (include != null)
            {
                query = include(query);
            }

            return await query.FirstOrDefaultAsync();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public async Task<T> AddAsync(T entity)
        {
            await dbSet.AddAsync(entity);

            return entity;
        }

        public void AddRange(List<T> entity)
        {
            dbSet.AddRange(entity);
        }

        public void Update(T entity)
        {
            dbSet.Attach(entity);

            _context.Entry(entity).State = EntityState.Modified;
        }

        public Task<T> UpdateAsync(T entity)
        {
            dbSet.Attach(entity);

            _context.Entry(entity).State = EntityState.Modified;

            return Task.FromResult(entity);
        }

        public void Delete(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }

            dbSet.Remove(entity);
        }

        public Task<T> DeleteAsync(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }

            dbSet.Remove(entity);

            return Task.FromResult(entity);
        }

        public void DeleteRange(List<T> entityList)
        {
            dbSet.RemoveRange(entityList);
        }

        public bool Exists(Expression<Func<T, bool>> filter = null)
        {
            if (filter == null)
            {
                return dbSet.Any();
            }

            return dbSet.Any(filter);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        public async Task<IEnumerable<T>> GetAllAsync(
     Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
     Expression<Func<T, bool>> filter = null,
     bool disabledTracking = true)
        {
            IQueryable<T> query = dbSet;

            if (disabledTracking)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }

                disposed = true;
            }
        }
        
    }
}