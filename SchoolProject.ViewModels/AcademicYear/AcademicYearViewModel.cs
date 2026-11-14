using SchoolProject.Models;
using AcademicYearModel = SchoolProject.Models.AcademicYear;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels.AcademicYear
{
    public class AcademicYearViewModel
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string DisplayName =>
            $"{StartDate.Year} - {EndDate.Year}";

        public bool IsCurrent { get; set; }

        public AcademicYearStatus Status { get; set; }

        public int TotalSessions { get; set; }

        public List<SessionViewModel> Sessions { get; set; }
            = new();
        public string? ClosedByUserId { get; set; }

        public DateTime? ClosedAt { get; set; }

        public string? CloseConfirmation { get; set; }

        public string? CloseSignature { get; set; }
        bool includeCloseSignature = false;

        public AcademicYearViewModel()
        {

        }

        public AcademicYearViewModel(AcademicYearModel academicYear)
        {
            Id = academicYear.Id;
            StartDate = academicYear.StartDate;
            EndDate = academicYear.EndDate;
            IsCurrent = academicYear.IsCurrent;
            Status = academicYear.Status;

            TotalSessions = academicYear.Sessions?.Count ?? 0;

            ClosedByUserId = academicYear.ClosedByUserId;

            ClosedAt = academicYear.ClosedAt;

            CloseConfirmation = academicYear.CloseConfirmation;

            CloseSignature = includeCloseSignature
                ? academicYear.CloseSignature
                : null;
            Sessions = academicYear.Sessions?
                .Select(x => new SessionViewModel(x))
                .ToList() ?? new List<SessionViewModel>();
        }
    }
}
