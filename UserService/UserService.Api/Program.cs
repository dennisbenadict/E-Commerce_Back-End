//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;
//using UserService.Api.Extensions;
//using FluentValidation;
//using UserService.Application.Validators;
//using UserService.Api.Consumers;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddUserService(builder.Configuration);

//// JWT
//var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default_secret_change_me");
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(opt =>
//    {
//        opt.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(key)
//        };
//    });

//builder.Services.AddAuthorization();

//// CORS
//builder.Services.AddCors(p =>
//    p.AddPolicy("AllowClient", pb => pb.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod())
//);

//// VALIDATION � ONLY ONE VALID LINE IN FLUENTVALIDATION V11+
//builder.Services.AddValidatorsFromAssembly(typeof(UpdateProfileValidator).Assembly);

//// BACKGROUND CONSUMER
//builder.Services.AddHostedService<UserRegisteredConsumer>();

//var app = builder.Build();

//app.UseGlobalExceptionHandling();

//app.UseSwagger();
//app.UseSwaggerUI();

//app.UseHttpsRedirection();
//app.UseCors("AllowClient");

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//app.Run();

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using UserService.Api.Extensions;
using UserService.Application.Validators;
using UserService.Api.Consumers;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Controllers
// --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --------------------
// Swagger + JWT (NO "Bearer" typing needed)
// --------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User Service API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste JWT token ONLY. Do NOT type 'Bearer'."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// --------------------
// App Services
// --------------------
builder.Services.AddUserService(builder.Configuration);

// --------------------
// JWT Authentication
// --------------------
var key = Encoding.UTF8.GetBytes(
    builder.Configuration["Jwt:Key"] ?? "default_secret_change_me"
);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
        
        // Read token from cookie if not in Authorization header
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // First try to get token from Authorization header
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                
                // If not in header, try to get from cookie
                if (string.IsNullOrEmpty(token))
                {
                    token = context.Request.Cookies["access_token"];
                }
                
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// --------------------
// CORS
// --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// --------------------
// FluentValidation (v11+ CORRECT WAY)
// --------------------
builder.Services.AddValidatorsFromAssembly(
    typeof(UpdateProfileValidator).Assembly
);

// --------------------
// RabbitMQ Consumers
// --------------------
builder.Services.AddHostedService<UserRegisteredConsumer>();
builder.Services.AddHostedService<UserDataSyncedConsumer>();

// ====================
// BUILD APP
// ====================
var app = builder.Build();

// --------------------
// Middleware
// --------------------
app.UseGlobalExceptionHandling();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API v1");
});

app.UseHttpsRedirection();
app.UseCors("AllowClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
