namespace PRN232.LMS.API.Models.Responses;

public class StudentResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public List<EnrollmentSummaryResponse>? Enrollments { get; set; }
}

public class StudentDetailResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public List<StudentEnrollmentResponse> Enrollments { get; set; } = new();
}

public class StudentEnrollmentResponse
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class EnrollmentSummaryResponse
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class StudentV1Response
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public List<EnrollmentSummaryResponse>? Enrollments { get; set; }
}

public class StudentV1DetailResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public List<StudentEnrollmentResponse> Enrollments { get; set; } = new();
}

public class StudentV2Response : StudentResponse { }

public class StudentV2DetailResponse : StudentDetailResponse { }
