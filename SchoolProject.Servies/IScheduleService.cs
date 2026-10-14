
using SchoolProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Servies
{
    public interface IScheduleService
    {
        Task<List<ScheduleViewModel>> GetAll();

        Task<ScheduleViewModel?> GetById(int id);

        Task Add(CreateScheduleViewModel vm);

        Task Update(int id, CreateScheduleViewModel vm);

        Task Delete(int id);
    }
}
