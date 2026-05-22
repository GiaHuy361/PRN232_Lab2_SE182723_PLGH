namespace PRN232.LMS.Services.Models;

public class CourseModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public SemesterSummaryModel? Semester { get; set; }
    public List<EnrollmentSummaryModel>? Enrollments { get; set; }
}

public class CourseDetailModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public SemesterSummaryModel? Semester { get; set; }
    public List<CourseEnrollmentModel> Enrollments { get; set; } = new();
}

public class CourseEnrollmentModel
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CourseSummaryModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
}
