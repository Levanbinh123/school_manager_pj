using SchoolProject.ViewModels;
using SchoolProject.ViewModels.AcademicYear;
using SchoolProject.ViewModels.AcademicYear.SchoolProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Servies.Academiyear
{
    public interface IAcademicYearService
    {
        Task<AcademicYearViewModel> Create(
            CreateAcademicYearViewModel vm);

        Task<AcademicYearViewModel> Update(
            int id,
            UpdateAcademicYearViewModel vm);

        IEnumerable<AcademicYearViewModel> GetAll();

        Task<AcademicYearViewModel?> GetById(int id);

        Task<AcademicYearViewModel?> GetCurrent();

        Task Activate(int id);

        Task Complete(int id);

        Task Close(
     int id,
     CloseAcademicYearViewModel vm,
     string adminUserId);
    }
}
