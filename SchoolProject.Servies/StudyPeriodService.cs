using SchoolProject.Models;
using SchoolProject.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Servies
{
    public class StudyPeriodService : IStudyPeriodService
    {
        private readonly IUnitOfWork _unitOfWork;
        public StudyPeriodService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<StudyPeriod>> GetAll()
        {
            var periods = await _unitOfWork
               .GenericRepository<StudyPeriod>()
               .GetAllAsync();

            return periods
                .OrderBy(x => x.PeriodNumber)
                .ToList();  
        }
        public async Task<List<StudyPeriod>> GetActivePeriods()
        {
            var periods = await _unitOfWork.GenericRepository<StudyPeriod>().GetAllAsync();
            return periods
                .Where(x => x.IsActive)
                .OrderBy(x => x.PeriodNumber)
                .ToList();
        }
        public async Task<StudyPeriod?> GetById(int id)
        {
            return await _unitOfWork
                .GenericRepository<StudyPeriod>()
                .GetByIdAsync(x => x.Id == id);
        }
        public async Task Add(StudyPeriod model)
        {
            ValidatePeriod(model);

            var repository =
                _unitOfWork.GenericRepository<StudyPeriod>();

            var periods = await repository.GetAllAsync();

            // Không cho trùng số tiết
            if (periods.Any(x =>
                x.PeriodNumber == model.PeriodNumber))
            {
                throw new Exception(
                    "Số tiết đã tồn tại.");
            }

            // Không cho trùng khung giờ
            if (periods.Any(x =>
                x.StartTime == model.StartTime &&
                x.EndTime == model.EndTime))
            {
                throw new Exception(
                    "Khung giờ học đã tồn tại.");
            }

            await repository.AddAsync(model);

            _unitOfWork.Save();
        }

        public async Task Update(int id, StudyPeriod model)
        {
            ValidatePeriod(model);

            var repository =
                _unitOfWork.GenericRepository<StudyPeriod>();

            var period = await repository.GetByIdAsync(
                x => x.Id == id);

            if (period == null)
            {
                throw new Exception(
                    "Không tìm thấy tiết học.");
            }

            var periods = await repository.GetAllAsync();

            // Bỏ qua chính tiết học đang cập nhật
            if (periods.Any(x =>
                x.Id != id &&
                x.PeriodNumber == model.PeriodNumber))
            {
                throw new Exception(
                    "Số tiết đã tồn tại.");
            }

            if (periods.Any(x =>
                x.Id != id &&
                x.StartTime == model.StartTime &&
                x.EndTime == model.EndTime))
            {
                throw new Exception(
                    "Khung giờ học đã tồn tại.");
            }

            period.Name = model.Name.Trim();
            period.PeriodNumber = model.PeriodNumber;
            period.StartTime = model.StartTime;
            period.EndTime = model.EndTime;
            period.IsActive = model.IsActive;

            repository.Update(period);

            _unitOfWork.Save();
        }

        public async Task Delete(int id)
        {
            var repository =
                _unitOfWork.GenericRepository<StudyPeriod>();

            var period = await repository.GetByIdAsync(
                x => x.Id == id);

            if (period == null)
            {
                throw new Exception(
                    "Không tìm thấy tiết học.");
            }

            repository.Delete(period);

            _unitOfWork.Save();
        }
        // =====================================================
        // VALIDATE
        // =====================================================
        private static void ValidatePeriod(StudyPeriod model)
        {
            if (model == null)
            {
                throw new Exception(
                    "Dữ liệu tiết học không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new Exception(
                    "Tên tiết học không được để trống.");
            }

            if (model.PeriodNumber <= 0)
            {
                throw new Exception(
                    "Số tiết phải lớn hơn 0.");
            }

            if (model.StartTime < TimeSpan.Zero ||
                model.StartTime >= TimeSpan.FromDays(1) ||
                model.EndTime < TimeSpan.Zero ||
                model.EndTime >= TimeSpan.FromDays(1))
            {
                throw new Exception(
                    "Giờ bắt đầu và kết thúc không hợp lệ.");
            }

            if (model.EndTime <= model.StartTime)
            {
                throw new Exception(
                    "Giờ kết thúc phải sau giờ bắt đầu.");
            }
        }

    }
}
