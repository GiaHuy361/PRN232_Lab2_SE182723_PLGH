using System;
using System.Collections.Generic;

namespace PRN232.LMS.Repositories.Entities;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public int SemesterId { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Semester Semester { get; set; } = null!;
}
