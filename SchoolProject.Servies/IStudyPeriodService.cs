using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolProject.Models;

namespace SchoolProject.Servies
{
    public interface IStudyPeriodService
    {
        Task<List<StudyPeriod>> GetAll();
        // Lấy các tiết học đang hoạt động
        Task<List<StudyPeriod>> GetActivePeriods();

        // Lấy tiết học theo ID
        Task<StudyPeriod?> GetById(int id);

        // Thêm tiết học
        Task Add(StudyPeriod model);

        // Cập nhật tiết học
        Task Update(int id, StudyPeriod model);

        // Xóa tiết học
        Task Delete(int id);
    }
}
