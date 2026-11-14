using SchoolProject.Models;


namespace SchoolProject.ViewModels
{
    public class SessionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public int MaxCredits { get; set; }

        public int AcademicYearId { get; set; }

        public string AcademicYearDisplayName { get; set; }
            = string.Empty;

        public int SemesterNumber { get; set; }

        public SessionStatus Status { get; set; }

        public SessionViewModel()
        {
        }

        public SessionViewModel(Session session)
        {
            Id = session.Id;
            Name = session.Name;
            Start = session.Start;
            End = session.End;
            MaxCredits = session.MaxCredits;

            AcademicYearId = session.AcademicYearId ?? 0;

            if (session.AcademicYearInfo != null)
            {
                AcademicYearDisplayName =
                    $"{session.AcademicYearInfo.StartDate.Year} - " +
                    $"{session.AcademicYearInfo.EndDate.Year}";
            }

            SemesterNumber = session.SemesterNumber;
            Status = session.Status;
        }
    }
}