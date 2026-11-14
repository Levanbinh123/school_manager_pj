
using Microsoft.EntityFrameworkCore.Storage;

namespace SchoolProject.Repositories
{
    public interface IUnitOfWork
    {
        IGenericRepository<T> GenericRepository<T>()
            where T : class;

        void Save();

        Task<int> SaveAsync(
            CancellationToken cancellationToken = default);

        Task<IDbContextTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken = default);
    }
}
