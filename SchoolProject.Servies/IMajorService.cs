using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public interface IMajorService
    {
        Task<List<MajorViewModel>> GetAll();
        Task<MajorViewModel?> GetById(int id);
        Task Add(CreateMajorViewModel vm);
        Task Update(int id, CreateMajorViewModel vm);
        Task Delete(int id);
    }
}