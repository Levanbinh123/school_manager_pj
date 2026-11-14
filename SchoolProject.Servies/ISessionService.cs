using SchoolProject.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolProject.Servies
{
    public interface ISessionService
    {
        Task<SessionViewModel> Add(CreateSessionViewModel vm);

        Task<SessionViewModel> Update(
            int sessionId,
            UpdateSessionViewModel vm);

        IEnumerable<SessionViewModel> GetAll();

        Task<SessionViewModel?> GetById(int sessionId);

        IEnumerable<SessionViewModel> GetByAcademicYear(
            int academicYearId);

        Task Activate(int sessionId);

        Task Complete(int sessionId);

        Task Delete(int sessionId);
    }
}