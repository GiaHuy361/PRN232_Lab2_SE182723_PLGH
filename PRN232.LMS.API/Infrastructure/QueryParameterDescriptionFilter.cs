using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PRN232.LMS.API.Infrastructure;

public class QueryParameterDescriptionFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null) return;

        var controllerName = context.ApiDescription.ActionDescriptor.RouteValues["controller"]?.ToLower() ?? "";

        // 1. Hide 'expand' parameter from GET /api/subjects
        var toRemove = operation.Parameters
            .Where(p => p.In == ParameterLocation.Query && p.Name.ToLower() == "expand" && controllerName == "subjects")
            .ToList();
        foreach (var p in toRemove)
        {
            operation.Parameters.Remove(p);
        }

        // 2. Adjust parameter names and descriptions
        foreach (var parameter in operation.Parameters)
        {
            if (parameter.In == ParameterLocation.Query)
            {
                var nameLower = parameter.Name.ToLower();
                if (nameLower == "search" || nameLower == "sort" || nameLower == "page" || nameLower == "size" || nameLower == "fields" || nameLower == "expand")
                {
                    // Convert query parameter name to lowercase in Swagger
                    parameter.Name = nameLower;

                    // Apply simplified query parameter descriptions
                    parameter.Description = nameLower switch
                    {
                        "search" => "Search by keyword.",
                        "sort" => "Sort by a field.",
                        "page" => "Page number.",
                        "size" => "Items per page.",
                        "fields" => "Choose fields to show.",
                        "expand" => GetExpandDescription(context),
                        _ => parameter.Description
                    };
                }
            }
            else if (parameter.In == ParameterLocation.Path)
            {
                var nameLower = parameter.Name.ToLower();
                if (nameLower == "courseid")
                {
                    parameter.Description = "Course ID.";
                }
                else if (nameLower == "semesterid")
                {
                    parameter.Description = "Semester ID.";
                }
                else if (nameLower == "studentid")
                {
                    parameter.Description = "Student ID.";
                }
            }
        }
    }

    private string GetExpandDescription(OperationFilterContext context)
    {
        var controllerName = context.ApiDescription.ActionDescriptor.RouteValues["controller"]?.ToLower() ?? "";
        var path = context.ApiDescription.RelativePath?.ToLower() ?? "";

        if (controllerName == "semesters")
        {
            if (path.Contains("courses"))
                return "Show semester or enrollments.";
            return "Show courses.";
        }
        if (controllerName == "students")
        {
            if (path.Contains("enrollments"))
                return "Show student or course.";
            return "Show enrollments.";
        }
        if (controllerName == "courses")
        {
            if (path.Contains("enrollments"))
                return "Show student or course.";
            return "Show semester or enrollments.";
        }
        if (controllerName == "enrollments")
        {
            return "Show student or course.";
        }
        return "Show related data.";
    }
}
