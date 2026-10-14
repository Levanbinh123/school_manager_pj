using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public interface IClassService
    {
        // =========================
        // CLASS
        // =========================

        Task AddClass(
            CreateClassViewModel vm);

        Task UpdateClass(
            int id,
            CreateClassViewModel vm);

        Task DeleteClass(
            int id);

        Task<ClassViewModel?> GetClassById(
            int id);

        Task<List<ClassViewModel>> GetAllClass();


        // =========================
        // CLASS SESSION
        // =========================

        Task<ClassSessionViewModel> AddClassSession(
            CreateClassSessionViewModel vm);

        Task<List<ClassSessionViewModel>> GetClassSessions(
            int classId);


        // =========================
        // ADD STUDENTS TO CLASS SESSION
        // =========================

        Task<int> AddStudentsToClassSession(
            int classSessionId,
            List<int> studentList);
    }
}