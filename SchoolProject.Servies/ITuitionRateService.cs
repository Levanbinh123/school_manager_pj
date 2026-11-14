
using SchoolProject.Models;

namespace SchoolProject.Servies
{
    public interface ITuitionRateService
    {
        Task<List<TuitionRate>> GetAllAsync();

        Task<TuitionRate?> GetBySessionIdAsync(int sessionId);

        Task<TuitionRate> CreateAsync(
            int sessionId,
            decimal pricePerCredit);

        Task<bool> UpdateAsync(
            int id,
            decimal pricePerCredit);

        Task<bool> DeleteAsync(int id);
    }
}
