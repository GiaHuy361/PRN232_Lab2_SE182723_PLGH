using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Infrastructure;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.API.Middleware;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Repositories.Implements;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Implements;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ────────────────────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    options.ReturnHttpNotAcceptable = true;
})
.AddXmlSerializerFormatters()
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value != null && e.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(er => er.ErrorMessage).ToArray()
            );

        var response = ApiResponse<object>.ErrorResponse("Validation failed", errors);
        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ── DbContext ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Authentication ─────────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? builder.Configuration["Jwt__Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("JWT secret is not configured. Set environment variable Jwt__Secret.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PRN232.LMS.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PRN232.LMS.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            // Skip the default challenge behavior of writing standard error headers / body
            context.HandleResponse();

            // Retain the standard Bearer authentication header challenge
            context.Response.Headers.Append("WWW-Authenticate", "Bearer");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            var apiResponse = ApiResponse<object>.ErrorResponse("Unauthorized");
            var json = System.Text.Json.JsonSerializer.Serialize(apiResponse, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
            await context.Response.WriteAsync(json);
        },
        OnForbidden = async context =>
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            var apiResponse = ApiResponse<object>.ErrorResponse("Forbidden");
            var json = System.Text.Json.JsonSerializer.Serialize(apiResponse, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
            await context.Response.WriteAsync(json);
        }
    };
});
builder.Services.AddAuthorization();

// ── API Versioning ─────────────────────────────────────────────────────────
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ── Swagger ────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PRN232.LMS.API - v1", Version = "v1" });
    c.SwaggerDoc("v2", new() { Title = "PRN232.LMS.API - v2", Version = "v2" });

    // DocInclusionPredicate checks if the endpoint matches docName (v1 or v2).
    // Versioned actions appear in their matching version document.
    // Legacy (non-versioned / neutral) actions appear in both v1 and v2 documents to ensure they are always testable!
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var metadata = apiDesc.ActionDescriptor.EndpointMetadata;
        var apiVersions = metadata.OfType<Asp.Versioning.ApiVersionAttribute>()
                                  .SelectMany(attr => attr.Versions)
                                  .ToList();

        if (apiVersions.Count > 0)
        {
            return apiVersions.Any(v => $"v{v.MajorVersion}" == docName);
        }

        // Include all non-versioned/neutral legacy actions in both docs
        return true;
    });

    c.OperationFilter<HideJsonIgnoreParameterFilter>(); // hides ExpandList / FieldList
    c.OperationFilter<QueryParameterDescriptionFilter>(); // lowercases and adds rich descriptions to query parameters
    c.OperationFilter<AuthorizeOperationFilter>(); // dynamically shows lock icons on protected endpoints only
    
    // Add Security Definition for Bearer JWT Authentication
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT access token."
    });
});

// ── Repositories ───────────────────────────────────────────────────────────
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

// ── Services ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// ── Middleware ─────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PRN232.LMS.API v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "PRN232.LMS.API v2");
    c.RoutePrefix = "swagger";
});

// Redirect root "/" → Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.UseHttpsRedirection();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
