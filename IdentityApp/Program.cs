using System.Text;
using IdentityApp.Data;
using IdentityApp.Services;
using IdentityApp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<Context>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddIdentityCore<User>(option =>
{
    option.Password.RequiredLength = 6;
    option.Password.RequireNonAlphanumeric = false;
    option.Password.RequireDigit = false;
    option.Password.RequireLowercase = false;
    option.Password.RequireUppercase = false;

    option.SignIn.RequireConfirmedEmail = true;

})
.AddRoles<IdentityRole>()
.AddRoleManager<RoleManager<IdentityRole>>()
.AddEntityFrameworkStores<Context>()
.AddSignInManager<SignInManager<User>>()
.AddUserManager<UserManager<User>>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidateIssuer = true,
            //ValidAudience = builder.Configuration["JWT:Audience"]
        };
    });

builder.Services.AddCors();

builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.
         ModelState.
         Where(x => x.Value.Errors.Count > 0).
         SelectMany(x => x.Value.Errors).
         Select(x => x.ErrorMessage).ToList();

        var toReturn = new
        {
            Errors = errors
        };
        return new BadRequestObjectResult(toReturn);
    };
});

var app = builder.Build();

app.UseCors(opt =>
{
    opt.AllowAnyHeader()
    //.AllowCredentials()
    .AllowAnyMethod()
    //.WithOrigins(builder.Configuration["JWT:ClientUrl"]);
    .AllowAnyOrigin();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
