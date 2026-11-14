using SchoolProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Servies
{
    public interface IAcademicReportService
    {
        Task<SessionReportViewModel> GetSessionReport(int sessionId);

        Task<AcademicYearReportViewModel> GetAcademicYearReport(
            int academicYearId);
    }
}
