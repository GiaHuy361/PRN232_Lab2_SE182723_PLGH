using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using PRN232.LMS.API.Infrastructure;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiVersionNeutral]
[Authorize]
[ApiController]
[Route("api/courses")]
[Produces("application/json", "application/xml")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _service;

    public CoursesController(ICourseService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CourseResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters query)
    {
        var (items, total) = await _service.GetAllAsync(query);
        var responses = items.Select(MapToResponse).ToList();

        IEnumerable<object> finalItems = query.FieldList.Count > 0
            ? responses.Select(r => FieldSelectionHelper.SelectFields(r, query.FieldList)).ToList()
            : responses.Cast<object>().ToList();

        var paged = new PagedResponse<object>
        {
            Items = finalItems,
            Pagination = new PaginationMetadata
            {
                Page = query.Page,
                PageSize = query.Size,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling((double)total / query.Size)
            }
        };
        return Ok(ApiResponse<PagedResponse<object>>.SuccessResponse(paged));
    }

    [HttpGet("{courseId:int:min(1)}/enrollments")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<EnrollmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEnrollmentsByCourseId([FromRoute] int courseId, [FromQuery] QueryParameters query)
    {
        var result = await _service.GetEnrollmentsByCourseIdAsync(courseId, query);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Course not found"));

        var responses = result.Value.Items.Select(e => new EnrollmentResponse
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId,
            CourseId = e.CourseId,
            EnrollDate = e.EnrollDate,
            Status = e.Status,
            Student = e.Student == null ? null : new StudentSummaryResponse
            {
                StudentId = e.Student.StudentId,
                FullName = e.Student.FullName,
                Email = e.Student.Email
            },
            Course = e.Course == null ? null : new CourseSummaryResponse
            {
                CourseId = e.Course.CourseId,
                CourseName = e.Course.CourseName
            }
        }).ToList();

        IEnumerable<object> finalItems = query.FieldList.Count > 0
            ? responses.Select(r => FieldSelectionHelper.SelectFields(r, query.FieldList)).ToList()
            : responses.Cast<object>().ToList();

        var paged = new PagedResponse<object>
        {
            Items = finalItems,
            Pagination = new PaginationMetadata
            {
                Page = query.Page,
                PageSize = query.Size,
                TotalItems = result.Value.TotalItems,
                TotalPages = (int)Math.Ceiling((double)result.Value.TotalItems / query.Size)
            }
        };
        return Ok(ApiResponse<PagedResponse<object>>.SuccessResponse(paged));
    }

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(ApiResponse<CourseDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var model = await _service.GetByIdAsync(id);
        if (model == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Course not found"));

        return Ok(ApiResponse<CourseDetailResponse>.SuccessResponse(MapToDetailResponse(model)));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.CourseName))
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request payload. CourseName is required."));

        var model = new CourseModel { CourseName = request.CourseName, SemesterId = request.SemesterId };
        var id = await _service.CreateAsync(model);
        var created = await _service.GetByIdAsync(id);
        return CreatedAtAction(nameof(GetById), new { id },
            ApiResponse<CourseDetailResponse>.SuccessResponse(MapToDetailResponse(created!), "Course created successfully"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCourseRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.CourseName))
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request payload. CourseName is required."));

        var model = new CourseModel { CourseName = request.CourseName, SemesterId = request.SemesterId };
        var result = await _service.UpdateAsync(id, model);
        if (!result)
            return NotFound(ApiResponse<object>.ErrorResponse("Course not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Course updated successfully"));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<object>.ErrorResponse("Course not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Course deleted successfully"));
    }

    private static CourseResponse MapToResponse(CourseModel m) => new()
    {
        CourseId = m.CourseId,
        CourseName = m.CourseName,
        SemesterId = m.SemesterId,
        Semester = m.Semester == null ? null : new SemesterSummaryResponse
        {
            SemesterId = m.Semester.SemesterId,
            SemesterName = m.Semester.SemesterName
        },
        Enrollments = m.Enrollments?.Select(e => new EnrollmentSummaryResponse
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId,
            CourseId = e.CourseId,
            EnrollDate = e.EnrollDate,
            Status = e.Status
        }).ToList()
    };

    private static CourseDetailResponse MapToDetailResponse(CourseDetailModel m) => new()
    {
        CourseId = m.CourseId,
        CourseName = m.CourseName,
        Semester = m.Semester == null ? null : new SemesterSummaryResponse
        {
            SemesterId = m.Semester.SemesterId,
            SemesterName = m.Semester.SemesterName
        },
        Enrollments = m.Enrollments.Select(e => new CourseEnrollmentResponse
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId,
            StudentName = e.StudentName,
            EnrollDate = e.EnrollDate,
            Status = e.Status
        }).ToList()
    };

    [HttpGet("{courseId:int:min(1)}/students")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<StudentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStudentsByCourseId(
        [FromRoute] int courseId,
        [FromQuery] QueryParameters query)
    {
        var result = await _service.GetStudentsByCourseIdAsync(courseId, query);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Course not found"));

        var responses = result.Value.Items.Select(MapToStudentResponse).ToList();

        IEnumerable<object> finalItems = query.FieldList.Count > 0
            ? responses.Select(r => FieldSelectionHelper.SelectFields(r, query.FieldList)).ToList()
            : responses.Cast<object>().ToList();

        var paged = new PagedResponse<object>
        {
            Items = finalItems,
            Pagination = new PaginationMetadata
            {
                Page = query.Page,
                PageSize = query.Size,
                TotalItems = result.Value.TotalItems,
                TotalPages = (int)Math.Ceiling((double)result.Value.TotalItems / query.Size)
            }
        };

        return Ok(ApiResponse<PagedResponse<object>>.SuccessResponse(paged));
    }

    private static StudentResponse MapToStudentResponse(StudentModel m) => new()
    {
        StudentId = m.StudentId,
        FullName = m.FullName,
        Email = m.Email,
        DateOfBirth = m.DateOfBirth,
        Phone = m.Phone,
        Enrollments = m.Enrollments?.Select(e => new EnrollmentSummaryResponse
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId,
            CourseId = e.CourseId,
            EnrollDate = e.EnrollDate,
            Status = e.Status
        }).ToList()
    };
}
