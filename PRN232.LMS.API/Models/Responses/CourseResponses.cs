namespace PRN232.LMS.API.Models.Responses;

public class CourseResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public SemesterSummaryResponse? Semester { get; set; }
    public List<EnrollmentSummaryResponse>? Enrollments { get; set; }
}

public class CourseDetailResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public SemesterSummaryResponse? Semester { get; set; }
    public List<CourseEnrollmentResponse> Enrollments { get; set; } = new();
}

public class SemesterSummaryResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
}

public class CourseEnrollmentResponse
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
