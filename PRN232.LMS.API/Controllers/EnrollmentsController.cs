using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/enrollments")]
[Produces("application/json")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters query)
    {
        try
        {
            var (items, total) = await _service.GetAllAsync(query);
            var responses = items.Select(MapToResponse).ToList();

            IEnumerable<object> finalItems = query.FieldList.Count > 0
                ? responses.Select(r => SelectFields(r, query.FieldList)).ToList()
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
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred.", ex.Message));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var model = await _service.GetByIdAsync(id);
            if (model == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Enrollment not found"));

            return Ok(ApiResponse<EnrollmentDetailResponse>.SuccessResponse(MapToDetailResponse(model)));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred.", ex.Message));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        try
        {
            var model = new EnrollmentModel
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                EnrollDate = request.EnrollDate,
                Status = request.Status
            };
            var id = await _service.CreateAsync(model);
            var created = await _service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id },
                ApiResponse<EnrollmentDetailResponse>.SuccessResponse(MapToDetailResponse(created!), "Enrollment created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred.", ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEnrollmentRequest request)
    {
        try
        {
            var model = new EnrollmentModel
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                EnrollDate = request.EnrollDate,
                Status = request.Status
            };
            var result = await _service.UpdateAsync(id, model);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Enrollment not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Enrollment updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred.", ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Enrollment not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Enrollment deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred.", ex.Message));
        }
    }

    // ── Mapping ────────────────────────────────────────────────────────────

    private static EnrollmentResponse MapToResponse(EnrollmentModel m) => new()
    {
        EnrollmentId = m.EnrollmentId,
        StudentId = m.StudentId,
        CourseId = m.CourseId,
        EnrollDate = m.EnrollDate,
        Status = m.Status,
        Student = m.Student == null ? null : new StudentSummaryResponse
        {
            StudentId = m.Student.StudentId,
            FullName = m.Student.FullName,
            Email = m.Student.Email
        },
        Course = m.Course == null ? null : new CourseSummaryResponse
        {
            CourseId = m.Course.CourseId,
            CourseName = m.Course.CourseName
        }
    };

    private static EnrollmentDetailResponse MapToDetailResponse(EnrollmentDetailModel m) => new()
    {
        EnrollmentId = m.EnrollmentId,
        EnrollDate = m.EnrollDate,
        Status = m.Status,
        Student = m.Student == null ? null : new StudentSummaryResponse
        {
            StudentId = m.Student.StudentId,
            FullName = m.Student.FullName,
            Email = m.Student.Email
        },
        Course = m.Course == null ? null : new CourseSummaryResponse
        {
            CourseId = m.Course.CourseId,
            CourseName = m.Course.CourseName
        }
    };

    private static object SelectFields(EnrollmentResponse r, List<string> fields)
    {
        var dict = new Dictionary<string, object?>();
        if (fields.Contains("enrollmentid")) dict["enrollmentId"] = r.EnrollmentId;
        if (fields.Contains("studentid")) dict["studentId"] = r.StudentId;
        if (fields.Contains("courseid")) dict["courseId"] = r.CourseId;
        if (fields.Contains("enrolldate")) dict["enrollDate"] = r.EnrollDate;
        if (fields.Contains("status")) dict["status"] = r.Status;
        if (r.Student != null) dict["student"] = r.Student;
        if (r.Course != null) dict["course"] = r.Course;
        return dict.Count > 0 ? dict : (object)r;
    }
}
