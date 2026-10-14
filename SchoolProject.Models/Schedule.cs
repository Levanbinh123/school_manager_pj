using SchoolProject.Models;

public class Schedule
{
    public int Id { get; set; }

    public int ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; }

    public int SubjectId { get; set; }
    public Subject Subject { get; set; }

    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; }

    public DayOfWeek Day { get; set; }

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public string Room { get; set; }
}