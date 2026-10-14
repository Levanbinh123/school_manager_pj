
using SchoolProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Servies
{
    public interface ISubjectService
    {
        Task<List<SubjectViewModel>> GetAll();

        Task<SubjectViewModel?> GetById(int id);

        Task Add(CreateSubjectViewModel vm);

        Task Update(int id, CreateSubjectViewModel vm);

        Task Delete(int id);
    }
}
