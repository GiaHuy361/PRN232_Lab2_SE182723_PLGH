using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Infrastructure;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/students")]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IStudentService service, ILogger<StudentsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<StudentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters query)
    {
        try
        {
            var (items, total) = await _service.GetAllAsync(query);
            var responses = items.Select(MapToResponse).ToList();

            // Field selection
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAll Students");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    [HttpGet("{studentId:int}/enrollments")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<EnrollmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEnrollmentsByStudentId(int studentId, [FromQuery] QueryParameters query)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetEnrollmentsByStudentId for student {StudentId}", studentId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var model = await _service.GetByIdAsync(id);
            if (model == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Student not found"));

            return Ok(ApiResponse<StudentDetailResponse>.SuccessResponse(MapToDetailResponse(model)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetById Student {Id}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request payload. FullName and Email are required."));

            var model = new StudentModel
            {
                FullName = request.FullName,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth
            };
            var id = await _service.CreateAsync(model);
            var created = await _service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id },
                ApiResponse<StudentDetailResponse>.SuccessResponse(MapToDetailResponse(created!), "Student created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Create Student");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request payload. FullName and Email are required."));

            var model = new StudentModel
            {
                FullName = request.FullName,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth
            };
            var result = await _service.UpdateAsync(id, model);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Student not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Student updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Update Student {Id}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Student not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Student deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Delete Student {Id}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    private static StudentResponse MapToResponse(StudentModel m) => new()
    {
        StudentId = m.StudentId,
        FullName = m.FullName,
        Email = m.Email,
        DateOfBirth = m.DateOfBirth,
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
