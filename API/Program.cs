using Microsoft.EntityFrameworkCore;
using Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors(myAllowSpecificOrigins);

app.Run();
