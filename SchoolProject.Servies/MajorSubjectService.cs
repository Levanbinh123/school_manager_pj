using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class MajorSubjectService : IMajorSubjectService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MajorSubjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ==========================================
        // 1. GÁN MÔN HỌC VÀO NGÀNH
        // ==========================================
        public async Task AssignSubject(
            AssignSubjectToMajorViewModel vm)
        {
            if (vm == null)
                throw new Exception(
                    "Thông tin gán môn học không hợp lệ.");

            if (vm.MajorId <= 0 || vm.SubjectId <= 0)
                throw new Exception(
                    "MajorId hoặc SubjectId không hợp lệ.");

            if (vm.YearLevel < 1 || vm.YearLevel > 4)
                throw new Exception(
                    "Năm học phải từ 1 đến 4.");

            var majorRepository =
                _unitOfWork.GenericRepository<Major>();

            var subjectRepository =
                _unitOfWork.GenericRepository<Subject>();

            var majorSubjectRepository =
                _unitOfWork.GenericRepository<MajorSubject>();

            var major = await majorRepository.GetByIdAsync(
                x => x.Id == vm.MajorId);

            if (major == null)
                throw new Exception("Không tìm thấy ngành học.");

            var subject = await subjectRepository.GetByIdAsync(
                x => x.Id == vm.SubjectId);

            if (subject == null)
                throw new Exception("Không tìm thấy môn học.");

            var exists = majorSubjectRepository.Exists(x =>
                x.MajorId == vm.MajorId &&
                x.SubjectId == vm.SubjectId);

            if (exists)
                throw new Exception(
                    "Môn học đã được gán cho ngành này.");

            var majorSubject = new MajorSubject
            {
                MajorId = vm.MajorId,
                SubjectId = vm.SubjectId,
                YearLevel = vm.YearLevel
            };

            await majorSubjectRepository.AddAsync(majorSubject);

            _unitOfWork.Save();
        }

        // ==========================================
        // 2. GỠ MÔN HỌC KHỎI NGÀNH
        // ==========================================
        public async Task RemoveSubject(
            int majorId,
            int subjectId)
        {
            if (majorId <= 0 || subjectId <= 0)
                throw new Exception(
                    "MajorId hoặc SubjectId không hợp lệ.");

            var repository =
                _unitOfWork.GenericRepository<MajorSubject>();

            var majorSubject = await repository.GetByIdAsync(x =>
                x.MajorId == majorId &&
                x.SubjectId == subjectId);

            if (majorSubject == null)
                throw new Exception(
                    "Môn học chưa được gán cho ngành này.");

            repository.Delete(majorSubject);

            _unitOfWork.Save();
        }

        // ==========================================
        // 3. LẤY TOÀN BỘ MÔN HỌC ĐÃ GÁN
        // ==========================================
        public async Task<List<MajorSubjectViewModel>>
            GetAllMajorSubjects()
        {
            var repository =
                _unitOfWork.GenericRepository<MajorSubject>();

            var list = repository.GetAll(
                include: query => query
                    .Include(x => x.Major)
                    .Include(x => x.Subject)
            )
            .OrderBy(x => x.MajorId)
            .ThenBy(x => x.YearLevel)
            .ThenBy(x => x.SubjectId)
            .ToList();

            return list.Select(x => new MajorSubjectViewModel
            {
                MajorId = x.MajorId,
                MajorName = x.Major.Name,

                SubjectId = x.SubjectId,
                SubjectName = x.Subject.Name,

                YearLevel = x.YearLevel
            }).ToList();
        }

        // ==========================================
        // 4. LẤY MÔN HỌC THEO NGÀNH
        // ==========================================
        public async Task<List<MajorSubjectViewModel>>
            GetMajorSubjectsByMajorId(int majorId)
        {
            if (majorId <= 0)
                throw new Exception("MajorId không hợp lệ.");

            var majorRepository =
                _unitOfWork.GenericRepository<Major>();

            var major = await majorRepository.GetByIdAsync(
                x => x.Id == majorId);

            if (major == null)
                throw new KeyNotFoundException(
                    "Không tìm thấy ngành học.");

            var repository =
                _unitOfWork.GenericRepository<MajorSubject>();

            var list = repository.GetAll(
                include: query => query
                    .Include(x => x.Major)
                    .Include(x => x.Subject)
            )
            .Where(x => x.MajorId == majorId)
            .OrderBy(x => x.YearLevel)
            .ThenBy(x => x.SubjectId)
            .ToList();

            return list.Select(x => new MajorSubjectViewModel
            {
                MajorId = x.MajorId,
                MajorName = x.Major.Name,

                SubjectId = x.SubjectId,
                SubjectName = x.Subject.Name,

                YearLevel = x.YearLevel
            }).ToList();
        }
    }
}