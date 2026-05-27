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
[Route("api/enrollments")]
[Produces("application/json", "application/xml")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<EnrollmentResponse>>), StatusCodes.Status200OK)]
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

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var model = await _service.GetByIdAsync(id);
        if (model == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Enrollment not found"));

        return Ok(ApiResponse<EnrollmentDetailResponse>.SuccessResponse(MapToDetailResponse(model)));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        if (request == null || request.StudentId <= 0 || request.CourseId <= 0)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request payload. StudentId and CourseId are required."));

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

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateEnrollmentRequest request)
    {
        if (request == null || request.StudentId <= 0 || request.CourseId <= 0)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request payload. StudentId and CourseId are required."));

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
            return NotFound(ApiResponse<object>.ErrorResponse("Enrollment not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Enrollment deleted successfully"));
    }

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
}
