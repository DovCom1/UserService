using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using UserService.Api.Middleware;
using UserService.Contract.Managers;
using UserService.Contract.Repositories;
using UserService.Data.Core;
using UserService.Data.Repositories;
using UserService.Model.Mappers;
using UserService.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DataBaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<DtoMapper>();
});

builder.Services.AddControllers();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<IEnemyRepository, EnemyRepository>();

builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<IFriendManager, FriendManager>();
builder.Services.AddScoped<IEnemyManager, EnemyManager>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRouting();
app.MapControllers();
app.Run();
