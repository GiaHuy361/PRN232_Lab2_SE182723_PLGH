using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PRN232.LMS.Services.Common;

public class QueryParameters
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Size must be between 1 and 100.")]
    public int Size { get; set; } = 10;

    public string? Search { get; set; }
    public string? Sort { get; set; }
    public string? Fields { get; set; }
    public string? Expand { get; set; }

    [JsonIgnore]
    public List<string> ExpandList =>
        string.IsNullOrWhiteSpace(Expand)
            ? new List<string>()
            : Expand.Split(',').Select(e => e.Trim().ToLower()).ToList();

    [JsonIgnore]
    public List<string> FieldList =>
        string.IsNullOrWhiteSpace(Fields)
            ? new List<string>()
            : Fields.Split(',').Select(f => f.Trim().ToLower()).ToList();
}
