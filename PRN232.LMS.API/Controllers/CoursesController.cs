using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Infrastructure;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/courses")]
[Produces("application/json")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _service;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(ICourseService service, ILogger<CoursesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CourseResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters query)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAll Courses");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    [HttpGet("{courseId:int}/enrollments")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<EnrollmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEnrollmentsByCourseId(int courseId, [FromQuery] QueryParameters query)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetEnrollmentsByCourseId for course {CourseId}", courseId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var model = await _service.GetByIdAsync(id);
            if (model == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Course not found"));

            return Ok(ApiResponse<CourseDetailResponse>.SuccessResponse(MapToDetailResponse(model)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetById Course {Id}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CourseName))
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request payload. CourseName is required."));

            var model = new CourseModel { CourseName = request.CourseName, SemesterId = request.SemesterId };
            var id = await _service.CreateAsync(model);
            var created = await _service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id },
                ApiResponse<CourseDetailResponse>.SuccessResponse(MapToDetailResponse(created!), "Course created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Create Course");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CourseName))
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request payload. CourseName is required."));

            var model = new CourseModel { CourseName = request.CourseName, SemesterId = request.SemesterId };
            var result = await _service.UpdateAsync(id, model);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Course not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Course updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Update Course {Id}", id);
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
                return NotFound(ApiResponse<object>.ErrorResponse("Course not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Course deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Delete Course {Id}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
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
}
