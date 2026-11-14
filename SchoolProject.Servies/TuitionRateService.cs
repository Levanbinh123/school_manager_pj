
using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using SchoolProject.Repositories;

namespace SchoolProject.Servies
{
    public class TuitionRateService : ITuitionRateService
    {
        private readonly ApplicationDbContext _context;

        public TuitionRateService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả mức học phí
        public async Task<List<TuitionRate>> GetAllAsync()
        {
            return await _context.Set<TuitionRate>()
                .Include(t => t.Session)
                .OrderByDescending(t => t.SessionId)
                .ToListAsync();
        }

        // Lấy mức học phí theo năm học
        public async Task<TuitionRate?> GetBySessionIdAsync(
            int sessionId)
        {
            return await _context.Set<TuitionRate>()
                .Include(t => t.Session)
                .FirstOrDefaultAsync(t => t.SessionId == sessionId);
        }

        // Tạo mức học phí cho năm học
        public async Task<TuitionRate> CreateAsync(
            int sessionId,
            decimal pricePerCredit)
        {
            if (pricePerCredit <= 0)
            {
                throw new ArgumentException(
                    "Giá mỗi tín chỉ phải lớn hơn 0.");
            }

            // Kiểm tra năm học có tồn tại
            bool sessionExists = await _context.Set<Session>()
                .AnyAsync(s => s.Id == sessionId);

            if (!sessionExists)
            {
                throw new ArgumentException(
                    "Năm học không tồn tại.");
            }

            // Mỗi năm học chỉ có một mức giá
            bool rateExists = await _context.Set<TuitionRate>()
                .AnyAsync(t => t.SessionId == sessionId);

            if (rateExists)
            {
                throw new InvalidOperationException(
                    "Năm học này đã có mức học phí.");
            }

            var tuitionRate = new TuitionRate
            {
                SessionId = sessionId,
                PricePerCredit = pricePerCredit
            };

            _context.Set<TuitionRate>().Add(tuitionRate);
            await _context.SaveChangesAsync();

            return tuitionRate;
        }

        // Cập nhật giá mỗi tín chỉ
        public async Task<bool> UpdateAsync(
            int id,
            decimal pricePerCredit)
        {
            if (pricePerCredit <= 0)
            {
                throw new ArgumentException(
                    "Giá mỗi tín chỉ phải lớn hơn 0.");
            }

            var tuitionRate = await _context.Set<TuitionRate>()
                .FindAsync(id);

            if (tuitionRate == null)
            {
                return false;
            }

            tuitionRate.PricePerCredit = pricePerCredit;

            await _context.SaveChangesAsync();

            return true;
        }

        // Xóa mức học phí
        public async Task<bool> DeleteAsync(int id)
        {
            var tuitionRate = await _context.Set<TuitionRate>()
                .FindAsync(id);

            if (tuitionRate == null)
            {
                return false;
            }

            _context.Set<TuitionRate>().Remove(tuitionRate);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
