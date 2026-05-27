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
[Route("api/students")]
[Produces("application/json", "application/xml")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<StudentResponse>>), StatusCodes.Status200OK)]
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

    [HttpGet("{studentId:int:min(1)}/enrollments")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<EnrollmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEnrollmentsByStudentId([FromRoute] int studentId, [FromQuery] QueryParameters query)
    {
        var result = await _service.GetEnrollmentsByStudentIdAsync(studentId, query);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Student not found"));

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
    [ProducesResponseType(typeof(ApiResponse<StudentDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromHeader(Name = "X-Request-Id")] string? requestId)
    {
        if (!string.IsNullOrWhiteSpace(requestId))
        {
            Response.Headers["X-Request-Id"] = requestId;
        }

        var model = await _service.GetByIdAsync(id);
        if (model == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Student not found"));

        return Ok(ApiResponse<StudentDetailResponse>.SuccessResponse(MapToDetailResponse(model)));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request payload. FullName and Email are required."));

        var model = new StudentModel
        {
            FullName = request.FullName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            Phone = request.Phone
        };
        var id = await _service.CreateAsync(model);
        var created = await _service.GetByIdAsync(id);
        return CreatedAtAction(nameof(GetById), new { id },
            ApiResponse<StudentDetailResponse>.SuccessResponse(MapToDetailResponse(created!), "Student created successfully"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStudentRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request payload. FullName and Email are required."));

        var model = new StudentModel
        {
            FullName = request.FullName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            Phone = request.Phone
        };
        var result = await _service.UpdateAsync(id, model);
        if (!result)
            return NotFound(ApiResponse<object>.ErrorResponse("Student not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Student updated successfully"));
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
            return NotFound(ApiResponse<object>.ErrorResponse("Student not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Student deleted successfully"));
    }

    private static StudentResponse MapToResponse(StudentModel m) => new()
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

    private static StudentDetailResponse MapToDetailResponse(StudentDetailModel m) => new()
    {
        StudentId = m.StudentId,
        FullName = m.FullName,
        Email = m.Email,
        DateOfBirth = m.DateOfBirth,
        Phone = m.Phone,
        Enrollments = m.Enrollments.Select(e => new StudentEnrollmentResponse
        {
            EnrollmentId = e.EnrollmentId,
            CourseId = e.CourseId,
            CourseName = e.CourseName,
            EnrollDate = e.EnrollDate,
            Status = e.Status
        }).ToList()
    };
}
