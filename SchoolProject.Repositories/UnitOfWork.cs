
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace SchoolProject.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy Repository
        public IGenericRepository<T> GenericRepository<T>()
            where T : class
        {
            return new GenericRepository<T>(_context);
        }

        // Lưu dữ liệu đồng bộ (giữ lại để tương thích code cũ)
        public void Save()
        {
            _context.SaveChanges();
        }

        // Lưu dữ liệu bất đồng bộ
        public async Task<int> SaveAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        // Bắt đầu Transaction
        public async Task<IDbContextTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(
                cancellationToken);
        }
    }
}
