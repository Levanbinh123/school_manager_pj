
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class SubjectService : ISubjectService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================
        // GET ALL SUBJECTS
        // =====================================================

        public async Task<List<SubjectViewModel>> GetAll()
        {
            var subjects = await _unitOfWork
                .GenericRepository<Subject>()
                .GetAllAsync();

            return subjects
                .Select(x => new SubjectViewModel(x))
                .ToList();
        }

        // =====================================================
        // GET SUBJECT BY ID
        // =====================================================

        public async Task<SubjectViewModel?> GetById(int id)
        {
            var subject = await _unitOfWork
                .GenericRepository<Subject>()
                .GetByIdAsync(x => x.Id == id);

            if (subject == null)
                return null;

            return new SubjectViewModel(subject);
        }

        // =====================================================
        // ADD SUBJECT
        // =====================================================

        public async Task Add(CreateSubjectViewModel vm)
        {
            if (vm == null)
                throw new Exception(
                    "Thông tin môn học không được để trống.");

            if (string.IsNullOrWhiteSpace(vm.Name))
                throw new Exception(
                    "Tên môn học không được để trống.");

            if (vm.Credits < 1 || vm.Credits > 10)
                throw new Exception(
                    "Số tín chỉ phải từ 1 đến 10.");

            var subjectRepository =
                _unitOfWork.GenericRepository<Subject>();

            var name = vm.Name.Trim();

            var exists = subjectRepository.Exists(
                x => x.Name == name);

            if (exists)
                throw new Exception(
                    "Tên môn học đã tồn tại trong danh mục.");

            var subject = new Subject
            {
                Name = name,
                Credits = vm.Credits
            };

            await subjectRepository.AddAsync(subject);

            _unitOfWork.Save();
        }

        // =====================================================
        // UPDATE SUBJECT
        // =====================================================

        public async Task Update(
            int id,
            CreateSubjectViewModel vm)
        {
            if (vm == null)
                throw new Exception(
                    "Thông tin môn học không được để trống.");

            if (string.IsNullOrWhiteSpace(vm.Name))
                throw new Exception(
                    "Tên môn học không được để trống.");

            if (vm.Credits < 1 || vm.Credits > 10)
                throw new Exception(
                    "Số tín chỉ phải từ 1 đến 10.");

            var subjectRepository =
                _unitOfWork.GenericRepository<Subject>();

            var subject = await subjectRepository.GetByIdAsync(
                x => x.Id == id);

            if (subject == null)
                throw new Exception("Không tìm thấy môn học.");

            var name = vm.Name.Trim();

            var exists = subjectRepository.Exists(x =>
                x.Name == name && x.Id != id);

            if (exists)
                throw new Exception(
                    "Tên môn học đã tồn tại trong danh mục.");

            subject.Name = name;
            subject.Credits = vm.Credits;

            subjectRepository.Update(subject);

            _unitOfWork.Save();
        }

        // =====================================================
        // DELETE SUBJECT
        // =====================================================

        public async Task Delete(int id)
        {
            var subjectRepository =
                _unitOfWork.GenericRepository<Subject>();

            var subject = await subjectRepository.GetByIdAsync(
                x => x.Id == id);

            if (subject == null)
                throw new Exception("Không tìm thấy môn học.");

            // -------------------------------------------------
            // 1. Kiểm tra MajorSubject
            // -------------------------------------------------

            var hasMajorSubject = _unitOfWork
                .GenericRepository<MajorSubject>()
                .Exists(x => x.SubjectId == id);

            if (hasMajorSubject)
            {
                throw new Exception(
                    "Không thể xóa môn học vì môn vẫn đang được gán cho ngành.");
            }

            // -------------------------------------------------
            // 2. Kiểm tra SessionSubject
            // -------------------------------------------------

            var hasSessionSubject = _unitOfWork
                .GenericRepository<SessionSubject>()
                .Exists(x => x.SubjectId == id);

            if (hasSessionSubject)
            {
                throw new Exception(
                    "Không thể xóa môn học vì môn đã được mở trong Session.");
            }

            // -------------------------------------------------
            // 3. Kiểm tra ClassSession
            // -------------------------------------------------

            var hasClassSession = _unitOfWork
                .GenericRepository<ClassSession>()
                .Exists(x => x.SubjectId == id);

            if (hasClassSession)
            {
                throw new Exception(
                    "Không thể xóa môn học vì môn đã có ClassSession.");
            }

            // -------------------------------------------------
            // 4. Kiểm tra Enroll
            // -------------------------------------------------

            var enrollIds = _unitOfWork
                .GenericRepository<Enroll>()
                .GetAll()
                .Where(x => x.SubjectId == id)
                .Select(x => x.Id)
                .ToList();

            if (enrollIds.Any())
            {
                throw new Exception(
                    "Không thể xóa môn học vì đã có sinh viên đăng ký.");
            }

            // -------------------------------------------------
            // 5. Kiểm tra Fee
            // -------------------------------------------------

            var hasFee = _unitOfWork
                .GenericRepository<Fee>()
                .Exists(x => x.SubjectId == id);

            if (hasFee)
            {
                throw new Exception(
                    "Không thể xóa môn học vì đã có dữ liệu học phí.");
            }

            // -------------------------------------------------
            // 6. Kiểm tra Assignment thông qua ClassSession
            // -------------------------------------------------

            // Assignment không còn SubjectId.
            // Tuy nhiên, ClassSession đã được kiểm tra ở bước 3.
            // Nếu đã tồn tại ClassSession của môn này,
            // không thể xóa Subject.

            // -------------------------------------------------
            // 7. Kiểm tra Schedule
            // -------------------------------------------------

            var hasSchedule = _unitOfWork
                .GenericRepository<Schedule>()
                .GetAll()
                .Any(x => x.SubjectId == id);

            if (hasSchedule)
            {
                throw new Exception(
                    "Không thể xóa môn học vì đã có lịch học.");
            }

            // -------------------------------------------------
            // 8. Xóa Subject
            // -------------------------------------------------

            subjectRepository.Delete(subject);

            _unitOfWork.Save();
        }
    }
}
