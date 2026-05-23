using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Infrastructure;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/semesters")]
[Produces("application/json")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _service;
    private readonly ILogger<SemestersController> _logger;

    public SemestersController(ISemesterService service, ILogger<SemestersController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<SemesterResponse>>), StatusCodes.Status200OK)]
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
            _logger.LogError(ex, "Unexpected error in GetAll Semesters");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    [HttpGet("{semesterId:int}/courses")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CourseResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCoursesBySemesterId(int semesterId, [FromQuery] QueryParameters query)
    {
        try
        {
            var result = await _service.GetCoursesBySemesterIdAsync(semesterId, query);
            if (result == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Semester not found"));

            var responses = result.Value.Items.Select(c => new CourseResponse
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                SemesterId = c.SemesterId,
                Semester = c.Semester == null ? null : new SemesterSummaryResponse
                {
                    SemesterId = c.Semester.SemesterId,
                    SemesterName = c.Semester.SemesterName
                },
                Enrollments = c.Enrollments?.Select(e => new EnrollmentSummaryResponse
                {
                    EnrollmentId = e.EnrollmentId,
                    StudentId = e.StudentId,
                    CourseId = e.CourseId,
                    EnrollDate = e.EnrollDate,
                    Status = e.Status
                }).ToList()
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
            _logger.LogError(ex, "Unexpected error in GetCoursesBySemesterId for semester {SemesterId}", semesterId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var model = await _service.GetByIdAsync(id);
            if (model == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Semester not found"));

            return Ok(ApiResponse<SemesterDetailResponse>.SuccessResponse(MapToDetailResponse(model)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetById Semester {Id}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateSemesterRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.SemesterName))
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request payload. SemesterName is required."));

            var model = new SemesterModel
            {
                SemesterName = request.SemesterName,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
            var id = await _service.CreateAsync(model);
            var created = await _service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id },
                ApiResponse<SemesterDetailResponse>.SuccessResponse(MapToDetailResponse(created!), "Semester created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Create Semester");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSemesterRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.SemesterName))
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request payload. SemesterName is required."));

            var model = new SemesterModel
            {
                SemesterName = request.SemesterName,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
            var result = await _service.UpdateAsync(id, model);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Semester not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Semester updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Update Semester {Id}", id);
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
                return NotFound(ApiResponse<object>.ErrorResponse("Semester not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Semester deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Delete Semester {Id}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
        }
    }

    private static SemesterResponse MapToResponse(SemesterModel m) => new()
    {
        SemesterId = m.SemesterId,
        SemesterName = m.SemesterName,
        StartDate = m.StartDate,
        EndDate = m.EndDate,
        Courses = m.Courses?.Select(c => new CourseSummaryResponse
        {
            CourseId = c.CourseId,
            CourseName = c.CourseName
        }).ToList()
    };

    private static SemesterDetailResponse MapToDetailResponse(SemesterDetailModel m) => new()
    {
        SemesterId = m.SemesterId,
        SemesterName = m.SemesterName,
        StartDate = m.StartDate,
        EndDate = m.EndDate,
        Courses = m.Courses.Select(c => new CourseSummaryResponse
        {
            CourseId = c.CourseId,
            CourseName = c.CourseName
        }).ToList()
    };
}
