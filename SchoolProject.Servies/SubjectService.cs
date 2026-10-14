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

        // =========================================================
        // GET ALL SUBJECTS
        // =========================================================
        public async Task<List<SubjectViewModel>> GetAll()
        {
            var subjects = await _unitOfWork
                .GenericRepository<Subject>()
                .GetAllAsync();

            return subjects
                .Select(x => new SubjectViewModel(x))
                .ToList();
        }

        // =========================================================
        // GET SUBJECT BY ID
        // =========================================================
        public async Task<SubjectViewModel?> GetById(int id)
        {
            var subject = await _unitOfWork
                .GenericRepository<Subject>()
                .GetByIdAsync(x => x.Id == id);

            if (subject == null)
            {
                return null;
            }

            return new SubjectViewModel(subject);
        }

        // =========================================================
        // ADD SUBJECT
        // =========================================================
        public async Task Add(CreateSubjectViewModel vm)
        {
            if (vm == null)
            {
                throw new Exception(
                    "Thông tin Subject không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(vm.Name))
            {
                throw new Exception(
                    "Tên Subject không được để trống.");
            }

            // -----------------------------------------------------
            // Repository Subject
            // -----------------------------------------------------
            var subjectRepository =
                _unitOfWork
                    .GenericRepository<Subject>();

            // -----------------------------------------------------
            // Kiểm tra Major
            // -----------------------------------------------------
            var major =
                _unitOfWork
                    .GenericRepository<Major>()
                    .GetById(vm.MajorId);

            if (major == null)
            {
                throw new Exception(
                    "Không tìm thấy Major.");
            }

            // -----------------------------------------------------
            // Chuẩn hóa tên
            // -----------------------------------------------------
            var name = vm.Name.Trim();

            // -----------------------------------------------------
            // Kiểm tra Subject trùng trong cùng Major
            // -----------------------------------------------------
            var exists =
                subjectRepository.Exists(
                    x =>
                        x.Name == name &&
                        x.MajorId == vm.MajorId);

            if (exists)
            {
                throw new Exception(
                    "Subject này đã tồn tại trong Major.");
            }

            // -----------------------------------------------------
            // Tạo Subject
            // -----------------------------------------------------
            var subject = new Subject
            {
                Name = name,
                MajorId = vm.MajorId
            };

            await subjectRepository.AddAsync(subject);

            _unitOfWork.Save();
        }

        // =========================================================
        // UPDATE SUBJECT
        // =========================================================
        public async Task Update(
            int id,
            CreateSubjectViewModel vm)
        {
            if (vm == null)
            {
                throw new Exception(
                    "Thông tin Subject không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(vm.Name))
            {
                throw new Exception(
                    "Tên Subject không được để trống.");
            }

            // -----------------------------------------------------
            // Repository Subject
            // -----------------------------------------------------
            var subjectRepository =
                _unitOfWork
                    .GenericRepository<Subject>();

            // -----------------------------------------------------
            // Tìm Subject
            // -----------------------------------------------------
            var subject =
                await subjectRepository
                    .GetByIdAsync(
                        x => x.Id == id);

            if (subject == null)
            {
                throw new Exception(
                    "Không tìm thấy Subject.");
            }

            // -----------------------------------------------------
            // Kiểm tra Major
            // -----------------------------------------------------
            var major =
                _unitOfWork
                    .GenericRepository<Major>()
                    .GetById(vm.MajorId);

            if (major == null)
            {
                throw new Exception(
                    "Không tìm thấy Major.");
            }

            // -----------------------------------------------------
            // Chuẩn hóa tên
            // -----------------------------------------------------
            var name = vm.Name.Trim();

            // -----------------------------------------------------
            // Kiểm tra trùng Subject
            // Không tính chính Subject đang update
            // -----------------------------------------------------
            var exists =
                subjectRepository.Exists(
                    x =>
                        x.Name == name &&
                        x.MajorId == vm.MajorId &&
                        x.Id != id);

            if (exists)
            {
                throw new Exception(
                    "Subject này đã tồn tại trong Major.");
            }

            // -----------------------------------------------------
            // Update
            // -----------------------------------------------------
            subject.Name = name;
            subject.MajorId = vm.MajorId;

            subjectRepository.Update(subject);

            _unitOfWork.Save();
        }

        // =========================================================
        // DELETE SUBJECT
        // =========================================================
        public async Task Delete(int id)
        {
            var subjectRepository =
                _unitOfWork
                    .GenericRepository<Subject>();

            // -----------------------------------------------------
            // Tìm Subject
            // -----------------------------------------------------
            var subject =
                await subjectRepository
                    .GetByIdAsync(
                        x => x.Id == id);

            if (subject == null)
            {
                throw new Exception(
                    "Không tìm thấy Subject.");
            }

            // -----------------------------------------------------
            // Kiểm tra SubjectYear
            // -----------------------------------------------------
            var hasSubjectYear =
                _unitOfWork
                    .GenericRepository<SubjectYear>()
                    .GetAll()
                    .Any(x =>
                        x.SubjectId == id);

            if (hasSubjectYear)
            {
                throw new Exception(
                    "Không thể xóa Subject vì Subject đã được cấu hình năm học.");
            }

            // -----------------------------------------------------
            // Kiểm tra SessionSubject
            // -----------------------------------------------------
            var hasSessionSubject =
                _unitOfWork
                    .GenericRepository<SessionSubject>()
                    .GetAll()
                    .Any(x =>
                        x.SubjectId == id);

            if (hasSessionSubject)
            {
                throw new Exception(
                    "Không thể xóa Subject vì Subject đã được mở trong Session.");
            }

            // -----------------------------------------------------
            // Kiểm tra Enroll
            // -----------------------------------------------------
            var hasEnroll =
                _unitOfWork
                    .GenericRepository<Enroll>()
                    .GetAll()
                    .Any(x =>
                        x.SubjectId == id);

            if (hasEnroll)
            {
                throw new Exception(
                    "Không thể xóa Subject vì đã có sinh viên đăng ký.");
            }

            // -----------------------------------------------------
            // Kiểm tra ClassSession
            // -----------------------------------------------------
            var hasClassSession =
                _unitOfWork
                    .GenericRepository<ClassSession>()
                    .GetAll()
                    .Any(x =>
                        x.SubjectId == id);

            if (hasClassSession)
            {
                throw new Exception(
                    "Không thể xóa Subject vì Subject đã có ClassSession.");
            }

            // -----------------------------------------------------
            // Xóa
            // -----------------------------------------------------
            subjectRepository.Delete(subject);

            _unitOfWork.Save();
        }
    }
}