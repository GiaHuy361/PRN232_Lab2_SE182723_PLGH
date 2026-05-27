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
[Route("api/semesters")]
[Produces("application/json", "application/xml")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _service;

    public SemestersController(ISemesterService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<SemesterResponse>>), StatusCodes.Status200OK)]
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

    [HttpGet("{semesterId:int:min(1)}/courses")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CourseResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCoursesBySemesterId([FromRoute] int semesterId, [FromQuery] QueryParameters query)
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

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var model = await _service.GetByIdAsync(id);
        if (model == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Semester not found"));

        return Ok(ApiResponse<SemesterDetailResponse>.SuccessResponse(MapToDetailResponse(model)));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateSemesterRequest request)
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

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateSemesterRequest request)
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
            return NotFound(ApiResponse<object>.ErrorResponse("Semester not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Semester deleted successfully"));
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
