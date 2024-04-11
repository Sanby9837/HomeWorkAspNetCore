using API.Middleware;
using Microsoft.EntityFrameworkCore;
using Server;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;

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

// IMemoryCache�A�Ȫ`�J
builder.Services.AddMemoryCache();

// MSSQL
var SqlConn = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new Exception("MSSQL �s�u���`���p���jK��");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(SqlConn));

// Redis 
var redisConn = builder.Configuration.GetSection("Redis")["ConnectionString"]
    ?? throw new Exception("Redis �s�u���`���p���p��");

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));

//IDatabase
builder.Services.AddSingleton<IDatabase>(provider =>
{
    var redisDb = provider.GetRequiredService<IConnectionMultiplexer>();
    return redisDb.GetDatabase();
});


// SeqLog
var seqConn = builder.Configuration.GetSection("Seq")["ServerUrl"]
    ?? throw new Exception("SeqLog �s�u���`���p���v��");

builder.Logging.AddSeq(seqConn);

// ���o���Pappsetting��Seq�r�����
Console.WriteLine($"I'm {env} Seq:{seqConn}");

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
