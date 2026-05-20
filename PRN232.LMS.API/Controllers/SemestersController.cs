using Microsoft.AspNetCore.Mvc;
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

    public SemestersController(ISemesterService service)
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
    [ProducesResponseType(typeof(ApiResponse<SemesterDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred.", ex.Message));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateSemesterRequest request)
    {
        try
        {
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
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred.", ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSemesterRequest request)
    {
        try
        {
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
                return NotFound(ApiResponse<object>.ErrorResponse("Semester not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Semester deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred.", ex.Message));
        }
    }

    private static SemesterResponse MapToResponse(SemesterModel m) => new()
    {
        SemesterId = m.SemesterId,
        SemesterName = m.SemesterName,
        StartDate = m.StartDate,
        EndDate = m.EndDate
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

    private static object SelectFields(SemesterResponse r, List<string> fields)
    {
        var dict = new Dictionary<string, object?>();
        if (fields.Contains("semesterid")) dict["semesterId"] = r.SemesterId;
        if (fields.Contains("semestername")) dict["semesterName"] = r.SemesterName;
        if (fields.Contains("startdate")) dict["startDate"] = r.StartDate;
        if (fields.Contains("enddate")) dict["endDate"] = r.EndDate;
        return dict.Count > 0 ? dict : (object)r;
    }
}
