using System.Reflection;

namespace PRN232.LMS.API.Infrastructure;

public static class FieldSelectionHelper
{
    public static object SelectFields<T>(T response, List<string> fields)
    {
        if (response == null) return null!;
        if (fields == null || fields.Count == 0) return response;

        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Standard related property names that represent expanded resources
        var expandedProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "student", "course", "semester", "enrollments", "courses"
        };

        foreach (var prop in properties)
        {
            var propNameLower = prop.Name.ToLower();

            // 1. If the field is explicitly selected, add it.
            // 2. OR, if it's a dynamic expanded resource AND it is NOT null (meaning it was populated),
            //    we MUST preserve it regardless of whether it's listed in fields.
            var isExplicitField = fields.Contains(propNameLower);
            var isExpandedResource = expandedProps.Contains(propNameLower);
            var propValue = prop.GetValue(response);

            if (isExplicitField || (isExpandedResource && propValue != null))
            {
                // CamelCase the key for JSON output
                var jsonKey = char.ToLower(prop.Name[0]) + prop.Name[1..];
                dict[jsonKey] = propValue;
            }
        }

        return dict.Count > 0 ? dict : response;
    }
}
