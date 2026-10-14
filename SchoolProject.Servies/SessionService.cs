using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SessionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ============================================================
        // ADD SESSION
        // ============================================================

        public async Task Add(CreateSessionViewModel vm)
        {
            if (vm == null)
            {
                throw new Exception(
                    "Thông tin Session không được để trống.");
            }

            var model = vm.Convert(vm);

            await _unitOfWork
                .GenericRepository<Session>()
                .AddAsync(model);

            _unitOfWork.Save();
        }

        // ============================================================
        // GET ALL - PAGING
        // ============================================================

        public PagedResult<SessionViewModel> GetAll(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            var repository =
                _unitOfWork.GenericRepository<Session>();

            int excludeRecords =
                (pageNumber - 1) * pageSize;

            var modelList = repository
                .GetAll()
                .OrderBy(x => x.Id)
                .Skip(excludeRecords)
                .Take(pageSize)
                .ToList();

            int totalCount = repository
                .GetAll()
                .Count();

            var vmList =
                ConvertModelToViewModelList(modelList);

            return new PagedResult<SessionViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // ============================================================
        // GET ALL - NO PAGING
        // ============================================================

        public IEnumerable<SessionViewModel> GetAll()
        {
            var modelList = _unitOfWork
                .GenericRepository<Session>()
                .GetAll()
                .OrderBy(x => x.Id)
                .ToList();

            return ConvertModelToViewModelList(
                modelList);
        }

        // ============================================================
        // GET BY ID
        // ============================================================

        public async Task<SessionViewModel?> GetById(
            int sessionId)
        {
            var session = await _unitOfWork
                .GenericRepository<Session>()
                .GetByIdAsync(
                    x => x.Id == sessionId);

            if (session == null)
            {
                return null;
            }

            return new SessionViewModel(session);
        }

        // ============================================================
        // CONVERT MODEL TO VIEW MODEL
        // ============================================================

        private List<SessionViewModel>
            ConvertModelToViewModelList(
                List<Session> modelList)
        {
            return modelList
                .Select(x =>
                    new SessionViewModel(x))
                .ToList();
        }
    }
}