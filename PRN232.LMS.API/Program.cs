using Microsoft.EntityFrameworkCore;
using PRN232.LMS.API.Infrastructure;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Repositories.Implements;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Implements;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ────────────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── DbContext ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// ── Services ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();

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
app.UseAuthorization();
app.MapControllers();

app.Run();
