using SchoolProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Servies
{
    public interface IMajorSubjectService
    {
        Task AssignSubject(AssignSubjectToMajorViewModel vm);

        Task RemoveSubject(int majorId, int subjectId);
        Task<List<MajorSubjectViewModel>> GetAllMajorSubjects();

        Task<List<MajorSubjectViewModel>>
            GetMajorSubjectsByMajorId(int majorId);
    }
}
