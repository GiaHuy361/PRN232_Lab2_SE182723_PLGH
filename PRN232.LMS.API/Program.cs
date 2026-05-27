using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Infrastructure;
using PRN232.LMS.API.Models.Responses;
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
});
builder.Services.AddAuthorization();

// ── Swagger ────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PRN232.LMS.API", Version = "1.0" });
    c.OperationFilter<HideJsonIgnoreParameterFilter>(); // hides ExpandList / FieldList
    c.OperationFilter<QueryParameterDescriptionFilter>(); // lowercases and adds rich descriptions to query parameters
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
    c.RoutePrefix = "swagger";
});

// Redirect root "/" → Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
