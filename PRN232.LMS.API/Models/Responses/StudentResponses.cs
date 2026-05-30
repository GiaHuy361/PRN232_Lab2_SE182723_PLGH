namespace PRN232.LMS.API.Models.Responses;

// ── Legacy / Current Student responses (returned by /api/students) ────────────
// Contains studentCode (V2 era fields)

public class StudentResponse
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public List<EnrollmentSummaryResponse>? Enrollments { get; set; }
}

public class StudentDetailResponse
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
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

// ── API Version 1 Student responses ─────────────────────────────────────────
// V1 contract: no phone, no studentCode (old contract)

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

// ── API Version 2 Student responses ─────────────────────────────────────────
// V2 contract: includes studentCode + phone

public class StudentV2Response
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public List<EnrollmentSummaryResponse>? Enrollments { get; set; }
}

public class StudentV2DetailResponse
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public List<StudentEnrollmentResponse> Enrollments { get; set; } = new();
}

// ── Student summary (used inside Enrollment responses) ───────────────────────
public class StudentSummaryResponse
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
