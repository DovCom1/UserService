using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UserService.Api.Middleware;
using UserService.Contract.Managers;
using UserService.Contract.Repositories;
using UserService.Contract.Services;
using UserService.Data.Core;
using UserService.Data.Repositories;
using UserService.Service.Managers;
using UserService.Service.Request;
using UserService.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DataBaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.Configure<RequestDomains>(builder.Configuration.GetSection("RequestDomains"));

builder.Services.AddSingleton<RequestFactory>();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<IEnemyRepository, EnemyRepository>();

builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<IFriendManager, FriendManager>();
builder.Services.AddScoped<IEnemyManager, EnemyManager>();

builder.Services.AddScoped<INotifierService, NotifierService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "User Service API", 
        Version = "v1",
        Description = "API для работы с User Service"
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service v1");
        c.RoutePrefix = "swagger";
    });
}
app.UseRouting();
app.MapControllers();
app.Run();
