
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class MajorService : IMajorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MajorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET ALL
        public async Task<List<MajorViewModel>> GetAll()
        {
            var majors = await _unitOfWork
                .GenericRepository<Major>()
                .GetAllAsync();

            return majors
                .Select(x => new MajorViewModel(x))
                .ToList();
        }

        // GET BY ID
        public async Task<MajorViewModel?> GetById(int id)
        {
            var major = await _unitOfWork
                .GenericRepository<Major>()
                .GetByIdAsync(x => x.Id == id);

            return major == null
                ? null
                : new MajorViewModel(major);
        }

        // ADD
        public async Task Add(CreateMajorViewModel vm)
        {
            if (vm == null)
                throw new Exception(
                    "Thông tin ngành không được để trống.");

            if (string.IsNullOrWhiteSpace(vm.Name))
                throw new Exception(
                    "Tên ngành không được để trống.");

            if (vm.RequiredCredits < 1 ||
                vm.RequiredCredits > 300)
            {
                throw new Exception(
                    "Số tín chỉ phải từ 1 đến 300.");
            }

            var repo = _unitOfWork.GenericRepository<Major>();
            var name = vm.Name.Trim();

            var exists = repo.Exists(x => x.Name == name);

            if (exists)
                throw new Exception("Tên ngành đã tồn tại.");

            var major = new Major
            {
                Name = name,
                RequiredCredits = vm.RequiredCredits
            };

            await repo.AddAsync(major);
            _unitOfWork.Save();
        }

        // UPDATE
        public async Task Update(
            int id,
            CreateMajorViewModel vm)
        {
            if (vm == null)
                throw new Exception(
                    "Thông tin ngành không được để trống.");

            if (string.IsNullOrWhiteSpace(vm.Name))
                throw new Exception(
                    "Tên ngành không được để trống.");

            if (vm.RequiredCredits < 1 ||
                vm.RequiredCredits > 300)
            {
                throw new Exception(
                    "Số tín chỉ phải từ 1 đến 300.");
            }

            var repo = _unitOfWork.GenericRepository<Major>();

            var major = await repo.GetByIdAsync(
                x => x.Id == id);

            if (major == null)
                throw new Exception("Không tìm thấy ngành học.");

            var name = vm.Name.Trim();

            var exists = repo.Exists(x =>
                x.Name == name && x.Id != id);

            if (exists)
                throw new Exception("Tên ngành đã tồn tại.");

            major.Name = name;
            major.RequiredCredits = vm.RequiredCredits;

            repo.Update(major);
            _unitOfWork.Save();
        }

        // DELETE
        public async Task Delete(int id)
        {
            var majorRepository =
                _unitOfWork.GenericRepository<Major>();

            var major = await majorRepository.GetByIdAsync(
                x => x.Id == id);

            if (major == null)
                throw new Exception("Không tìm thấy ngành học.");

            // Không xóa ngành còn sinh viên
            var hasStudents = _unitOfWork
                .GenericRepository<Student>()
                .Exists(x => x.MajorId == id);

            if (hasStudents)
            {
                throw new Exception(
                    "Không thể xóa ngành vì vẫn còn sinh viên thuộc ngành này.");
            }

            // Không xóa ngành còn môn học được gán
            var hasSubjects = _unitOfWork
                .GenericRepository<MajorSubject>()
                .Exists(x => x.MajorId == id);

            if (hasSubjects)
            {
                throw new Exception(
                    "Không thể xóa ngành vì vẫn còn môn học được gán. " +
                    "Hãy gỡ các môn học khỏi ngành trước.");
            }

            majorRepository.Delete(major);
            _unitOfWork.Save();
        }
    }
}
