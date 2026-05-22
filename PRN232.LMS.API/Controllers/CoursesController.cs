using Microsoft.AspNetCore.Mvc;
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

    public CoursesController(ICourseService service)
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
    [ProducesResponseType(typeof(ApiResponse<CourseDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred.", ex.Message));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        try
        {
            var model = new CourseModel { CourseName = request.CourseName, SemesterId = request.SemesterId };
            var id = await _service.CreateAsync(model);
            var created = await _service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id },
                ApiResponse<CourseDetailResponse>.SuccessResponse(MapToDetailResponse(created!), "Course created successfully"));
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
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseRequest request)
    {
        try
        {
            var model = new CourseModel { CourseName = request.CourseName, SemesterId = request.SemesterId };
            var result = await _service.UpdateAsync(id, model);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Course not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Course updated successfully"));
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
                return NotFound(ApiResponse<object>.ErrorResponse("Course not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Course deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred.", ex.Message));
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

    private static object SelectFields(CourseResponse r, List<string> fields)
    {
        var dict = new Dictionary<string, object?>();
        if (fields.Contains("courseid")) dict["courseId"] = r.CourseId;
        if (fields.Contains("coursename")) dict["courseName"] = r.CourseName;
        if (fields.Contains("semesterid")) dict["semesterId"] = r.SemesterId;
        if (r.Semester != null) dict["semester"] = r.Semester;
        if (r.Enrollments != null) dict["enrollments"] = r.Enrollments;
        return dict.Count > 0 ? dict : (object)r;
    }
}
