using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;

namespace PRN232.LMS.API.Infrastructure;

/// <summary>
/// Swagger operation filter that removes any [FromQuery] parameters
/// whose corresponding property is decorated with [JsonIgnore].
/// This keeps internal computed helpers (e.g. ExpandList, FieldList)
/// out of the Swagger UI.
/// </summary>
public class HideJsonIgnoreParameterFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null) return;

        var ignoredNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var actionParam in context.ApiDescription.ActionDescriptor.Parameters)
        {
            // Walk through all bound parameters' types looking for [JsonIgnore] props
            var paramType = actionParam.ParameterType;
            foreach (var prop in paramType.GetProperties())
            {
                if (prop.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true).Length > 0)
                    ignoredNames.Add(prop.Name);
            }
        }

        if (ignoredNames.Count == 0) return;

        var toRemove = operation.Parameters
            .Where(p => ignoredNames.Contains(p.Name))
            .ToList();

        foreach (var p in toRemove)
            operation.Parameters.Remove(p);
    }
}
