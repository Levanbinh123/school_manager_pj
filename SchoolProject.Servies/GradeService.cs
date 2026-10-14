//using SchoolProject.Models;
//using SchoolProject.Repositories;
//using SchoolProject.ViewModels;

//namespace SchoolProject.Servies
//{
//    public class GradeService : IGradeService
//    {
//        private readonly IUnitOfWork _unitOfWork;

//        public GradeService(IUnitOfWork unitOfWork)
//        {
//            _unitOfWork = unitOfWork;
//        }


//        // ============================================================
//        // ADD GRADE
//        // ============================================================

//        public async Task<GradeViewModel> Add(
//            CreateGradeViewModel vm)
//        {
//            // --------------------------------------------------------
//            // Validate
//            // --------------------------------------------------------

//            if (string.IsNullOrWhiteSpace(vm.Name))
//            {
//                throw new Exception(
//                    "Tên Grade không được để trống.");
//            }


//            // --------------------------------------------------------
//            // Check duplicate
//            // --------------------------------------------------------

//            var repository =
//                _unitOfWork.GenericRepository<Grade>();

//            var exists = repository
//                .GetAll()
//                .Any(x =>
//                    x.Name.ToLower() ==
//                    vm.Name.ToLower());

//            if (exists)
//            {
//                throw new Exception(
//                    "Grade này đã tồn tại.");
//            }


//            // --------------------------------------------------------
//            // Create
//            // --------------------------------------------------------

//            var grade = new Grade
//            {
//                Name = vm.Name
//            };


//            // --------------------------------------------------------
//            // Add
//            // --------------------------------------------------------

//            repository.Add(grade);


//            // --------------------------------------------------------
//            // Save
//            // --------------------------------------------------------

//            _unitOfWork.Save();


//            // --------------------------------------------------------
//            // Return
//            // --------------------------------------------------------

//            return new GradeViewModel(grade);
//        }


//        // ============================================================
//        // GET ALL - PAGING
//        // ============================================================

//        public async Task<PagedResult<GradeViewModel>> GetAll(
//            int pageNumber,
//            int pageSize)
//        {
//            if (pageNumber <= 0)
//            {
//                pageNumber = 1;
//            }

//            if (pageSize <= 0)
//            {
//                pageSize = 10;
//            }


//            var repository =
//                _unitOfWork.GenericRepository<Grade>();

//            var query = repository
//                .GetAll()
//                .OrderBy(x => x.Id);


//            var totalCount =
//                query.Count();


//            var grades = query
//                .Skip((pageNumber - 1) * pageSize)
//                .Take(pageSize)
//                .ToList();


//            var data = grades
//                .Select(x =>
//                    new GradeViewModel(x))
//                .ToList();


//            return new PagedResult<GradeViewModel>
//            {
                
//                PageNumber = pageNumber,
//                PageSize = pageSize
//            };
//        }


//        // ============================================================
//        // GET ALL - NO PAGING
//        // ============================================================

//        public async Task<List<GradeViewModel>> GetAll()
//        {
//            var repository =
//                _unitOfWork.GenericRepository<Grade>();

//            var grades = repository
//                .GetAll()
//                .OrderBy(x => x.Id)
//                .ToList();


//            return grades
//                .Select(x =>
//                    new GradeViewModel(x))
//                .ToList();
//        }


//        // ============================================================
//        // GET BY ID
//        // ============================================================

//        public async Task<GradeViewModel?> GetById(
//            int gradeId)
//        {
//            var repository =
//                _unitOfWork.GenericRepository<Grade>();

//            var grade = await repository
//                .GetByIdAsync(x =>
//                    x.Id == gradeId);

//            if (grade == null)
//            {
//                return null;
//            }


//            return new GradeViewModel(grade);
//        }
//    }
//}