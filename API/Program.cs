using System.Text;
using API.Database;
using API.Database.Interfaces;
using API.Middlewares;
using API.Model.Shared;
using API.Utils;
using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
  option.EnableAnnotations();
  option.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "API",
    Version = "v1"
  });
  option.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.Http,
    BearerFormat = "JWT",
    Scheme = "Bearer"
  });
  option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          },
        },
        []
      }
    });
});

builder.Services.AddSingleton<TokenProviders>();
builder.Services.AddCarter();
builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
builder.Services.AddScoped<AuthMiddleware>();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddMediatR(option =>
{
  option.RegisterServicesFromAssembly(typeof(Program).Assembly);
});
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
  option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
{
  option.RequireHttpsMetadata = false;
  option.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidAudience = builder.Configuration["JWT:Audience"],
    ValidIssuer = builder.Configuration["JWT:Issuer"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"] ?? throw new ArgumentNullException("JWT:SecretKey"))),
    RequireExpirationTime = true,
    ValidateIssuerSigningKey = false
  };
});
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
  app.MigrateDatabase<ApplicationDbContext>(async (_, _) => await Task.Delay(0));
}
else
{
  app.UseSwagger();
  app.UseSwaggerUI();
  app.MigrateDatabase<ApplicationDbContext>(async (_, _) => await Task.Delay(0));
}
app.UseHttpsRedirection();
app.UseMiddleware<AuthMiddleware>();
app.MapCarter();
app.UseAuthentication();
app.UseAuthorization();
app.Run();