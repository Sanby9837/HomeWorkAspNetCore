using API.Middleware;
using Microsoft.EntityFrameworkCore;
using Server;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment.EnvironmentName;
Console.WriteLine(env);
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: true);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var seqConn = builder.Configuration.GetSection("Seq")["ServerUrl"];
Console.WriteLine($"I'm {env} Seq:{seqConn}");
builder.Logging.AddSeq(seqConn);

builder.Services.AddScoped<LoggingMiddleware>();


var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
// �Ѱ����t�m
builder.Services.AddCors(options =>
{
    options.AddPolicy(myAllowSpecificOrigins, builder =>
    {
        builder.AllowAnyOrigin() // ���\����ӷ�
               .AllowAnyMethod()  // ���\���� HTTP ��k
               .AllowAnyHeader(); // ���\���� HTTP ���Y
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
