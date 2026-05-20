using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/subjects")]
[Produces("application/json")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _service;

    public SubjectsController(ISubjectService service)
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
    [ProducesResponseType(typeof(ApiResponse<SubjectDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var model = await _service.GetByIdAsync(id);
            if (model == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Subject not found"));

            return Ok(ApiResponse<SubjectDetailResponse>.SuccessResponse(MapToDetailResponse(model)));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred.", ex.Message));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubjectDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request)
    {
        try
        {
            var model = new SubjectModel
            {
                SubjectCode = request.SubjectCode,
                SubjectName = request.SubjectName,
                Credit = request.Credit
            };
            var id = await _service.CreateAsync(model);
            var created = await _service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id },
                ApiResponse<SubjectDetailResponse>.SuccessResponse(MapToDetailResponse(created!), "Subject created successfully"));
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
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubjectRequest request)
    {
        try
        {
            var model = new SubjectModel
            {
                SubjectCode = request.SubjectCode,
                SubjectName = request.SubjectName,
                Credit = request.Credit
            };
            var result = await _service.UpdateAsync(id, model);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Subject not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Subject updated successfully"));
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
                return NotFound(ApiResponse<object>.ErrorResponse("Subject not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Subject deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred.", ex.Message));
        }
    }

    private static SubjectResponse MapToResponse(SubjectModel m) => new()
    {
        SubjectId = m.SubjectId,
        SubjectCode = m.SubjectCode,
        SubjectName = m.SubjectName,
        Credit = m.Credit
    };

    private static SubjectDetailResponse MapToDetailResponse(SubjectDetailModel m) => new()
    {
        SubjectId = m.SubjectId,
        SubjectCode = m.SubjectCode,
        SubjectName = m.SubjectName,
        Credit = m.Credit
    };

    private static object SelectFields(SubjectResponse r, List<string> fields)
    {
        var dict = new Dictionary<string, object?>();
        if (fields.Contains("subjectid")) dict["subjectId"] = r.SubjectId;
        if (fields.Contains("subjectcode")) dict["subjectCode"] = r.SubjectCode;
        if (fields.Contains("subjectname")) dict["subjectName"] = r.SubjectName;
        if (fields.Contains("credit")) dict["credit"] = r.Credit;
        return dict.Count > 0 ? dict : (object)r;
    }
}
