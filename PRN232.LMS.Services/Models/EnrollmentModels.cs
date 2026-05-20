namespace PRN232.LMS.Services.Models;

public class EnrollmentModel
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class EnrollmentDetailModel
{
    public int EnrollmentId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public StudentSummaryModel? Student { get; set; }
    public CourseSummaryModel? Course { get; set; }
}
