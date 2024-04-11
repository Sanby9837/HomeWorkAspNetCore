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

// IMemoryCache服務注入
builder.Services.AddMemoryCache();

// MSSQL
var SqlConn = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new Exception("MSSQL 連線異常請聯絡大K哥");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(SqlConn));

// Redis 
var redisConn = builder.Configuration.GetSection("Redis")["ConnectionString"]
    ?? throw new Exception("Redis 連線異常請聯絡雷哥");

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));

//IDatabase
builder.Services.AddSingleton<IDatabase>(provider =>
{
    var redisDb = provider.GetRequiredService<IConnectionMultiplexer>();
    return redisDb.GetDatabase();
});


// SeqLog
var seqConn = builder.Configuration.GetSection("Seq")["ServerUrl"]
    ?? throw new Exception("SeqLog 連線異常請聯絡史哥");

builder.Logging.AddSeq(seqConn);

// 取得不同appsetting的Seq字串測試
Console.WriteLine($"I'm {env} Seq:{seqConn}");

builder.Services.AddScoped<LoggingMiddleware>();

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
