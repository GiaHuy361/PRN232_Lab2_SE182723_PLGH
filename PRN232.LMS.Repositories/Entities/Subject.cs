using System;
using System.Collections.Generic;

namespace PRN232.LMS.Repositories.Entities;

public partial class Subject
{
    public int SubjectId { get; set; }

    public string SubjectCode { get; set; } = null!;

    public string SubjectName { get; set; } = null!;

    public int Credit { get; set; }
}
