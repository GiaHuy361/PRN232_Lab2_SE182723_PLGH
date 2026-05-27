using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Infrastructure;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN232.LMS.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/students")]
[ApiController]
[Authorize]
[Produces("application/json", "application/xml")]
public class StudentsV1Controller : ControllerBase
{
    private readonly IStudentService _service;

    public StudentsV1Controller(IStudentService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<StudentV1Response>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters query)
    {
        var (items, total) = await _service.GetAllAsync(query);
        var responses = items.Select(MapToV1Response).ToList();

        // FieldSelectionHelper will naturally ignore "phone" since StudentV1Response does not contain it.
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
    [ProducesResponseType(typeof(ApiResponse<StudentV1DetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var model = await _service.GetByIdAsync(id);
        if (model == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Student not found"));

        return Ok(ApiResponse<StudentV1DetailResponse>.SuccessResponse(MapToV1DetailResponse(model)));
    }

    private static StudentV1Response MapToV1Response(StudentModel m) => new()
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

    private static StudentV1DetailResponse MapToV1DetailResponse(StudentDetailModel m) => new()
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
