
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public interface IGradeConfigurationService
    {
        Task<GradeConfigurationViewModel> Create(
            string userId,
            CreateGradeConfigurationViewModel vm);

        Task<GradeConfigurationViewModel> Update(
            string userId,
            int id,
            CreateGradeConfigurationViewModel vm);

        Task<GradeConfigurationViewModel> GetByClassSession(
            string userId,
            int classSessionId);
    }
}
