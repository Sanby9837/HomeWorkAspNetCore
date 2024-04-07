using API.Middleware;
using Microsoft.EntityFrameworkCore;
using Server;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Logging.AddSeq(builder.Configuration.GetSection("Seq")["ServerUrl"]);
builder.Services.AddScoped<LoggingMiddleware>();

var env = builder.Environment.EnvironmentName;
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: true);


var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
// 解除跨域配置
builder.Services.AddCors(options =>
{
    options.AddPolicy(myAllowSpecificOrigins, builder =>
    {
        builder.AllowAnyOrigin() // 允許任何來源
               .AllowAnyMethod()  // 允許任何 HTTP 方法
               .AllowAnyHeader(); // 允許任何 HTTP 標頭
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (env == "Development")
{
    Console.WriteLine("!!!!dev");
}


app.UseRouting();

app.UseMiddleware<LoggingMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", async context =>
    {
        var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        await context.Response.WriteAsync("Process Name:" + "{" + processName + "}");
    });
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors(myAllowSpecificOrigins);


app.Run();
