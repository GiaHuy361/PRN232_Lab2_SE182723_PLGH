namespace PRN232.LMS.Services.Models;

public class SemesterModel
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class SemesterDetailModel
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CourseSummaryModel> Courses { get; set; } = new();
}

public class SemesterSummaryModel
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
}
