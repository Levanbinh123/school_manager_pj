using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolProject.Models;

namespace SchoolProject.ViewModels
{
    public class CreateSessionViewModel
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int MaxCredits { get; set; } = 30;
        public string Name { get; set; } = string.Empty;
        public Session Convert(CreateSessionViewModel vm)
        {
            return new Session
            {
                Start = vm.Start,
                End=vm.End,
                MaxCredits = vm.MaxCredits,
                Name = vm.Name,
            };
        }
    }
}
