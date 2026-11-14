using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public interface IStudentService
    {
        Task AddStudent(CreateStudentViewModel student);

        PagedResult<StudentViewModel> GetAll(
            int pageNumber,
            int pageSize,
            string? search = null);

        Task<StudentViewModel?> GetById(int id);

        Task UpdateStudent(
            int id,
            CreateStudentViewModel student);

        Task DeleteStudent(int id);

        int GetAllStudents();
    }
}