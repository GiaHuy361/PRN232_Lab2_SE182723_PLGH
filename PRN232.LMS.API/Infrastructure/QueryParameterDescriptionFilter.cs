using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PRN232.LMS.API.Infrastructure;

public class QueryParameterDescriptionFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null) return;

        foreach (var parameter in operation.Parameters)
        {
            if (parameter.In == ParameterLocation.Query)
            {
                var nameLower = parameter.Name.ToLower();
                if (nameLower == "search" || nameLower == "sort" || nameLower == "page" || nameLower == "size" || nameLower == "fields" || nameLower == "expand")
                {
                    // Convert query parameter name to lowercase in Swagger
                    parameter.Name = nameLower;

                    // Apply unified query parameter descriptions
                    parameter.Description = nameLower switch
                    {
                        "search" => "Keyword or condition used to filter items.",
                        "sort" => "Comma-separated fields. Prefix '-' means descending, e.g. -enrollDate.",
                        "page" => "Page number starting from 1 (default is 1).",
                        "size" => "Number of items per page (default is 10, max is 100).",
                        "fields" => "Comma-separated scalar fields to return.",
                        "expand" => GetExpandDescription(context),
                        _ => parameter.Description
                    };
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
                return "Comma-separated related resources to include. Supported: semester, enrollments";
            return "Comma-separated related resources to include. Supported: courses";
        }
        if (controllerName == "students")
        {
            return "Comma-separated related resources to include. Supported: enrollments";
        }
        if (controllerName == "courses")
        {
            if (path.Contains("enrollments"))
                return "Comma-separated related resources to include. Supported: student, course";
            return "Comma-separated related resources to include. Supported: semester, enrollments";
        }
        if (controllerName == "enrollments")
        {
            return "Comma-separated related resources to include. Supported: student, course";
        }
        return "Comma-separated related resources to include.";
    }
}
