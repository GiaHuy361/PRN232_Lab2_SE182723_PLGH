using System.Text.Json.Serialization;

namespace PRN232.LMS.Services.Common;

public class QueryParameters
{
    private int _page = 1;
    private int _size = 10;

    public string? Search { get; set; }
    public string? Sort { get; set; }

    public int Page
    {
        get => _page;
        set => _page = value <= 0 ? 1 : value;
    }

    public int Size
    {
        get => _size;
        set => _size = value <= 0 ? 10 : (value > 100 ? 100 : value);
    }

    public string? Fields { get; set; }
    public string? Expand { get; set; }

    // Internal helpers — excluded from JSON serialization and Swagger
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
