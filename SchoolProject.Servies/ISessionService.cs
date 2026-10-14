using SchoolProject.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolProject.Servies
{
    public interface ISessionService
    {
        Task Add(
            CreateSessionViewModel vm);

        PagedResult<SessionViewModel> GetAll(
            int pageNumber,
            int pageSize);

        IEnumerable<SessionViewModel> GetAll();

        Task<SessionViewModel?> GetById(
            int sessionId);
    }
}