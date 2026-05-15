using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopEZ.UserService.Data;
using ShopEZ.UserService.Repositories;
using ShopEZ.UserService.Repositories.Interfaces;
using ShopEZ.UserService.Services;
using ShopEZ.UserService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────────────────
// 1.  CONTROLLERS + JSON
// ─────────────────────────────────────────────────────────────────────────────
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// ─────────────────────────────────────────────────────────────────────────────
// 2.  EF CORE — SQL SERVER
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("UserDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

// ─────────────────────────────────────────────────────────────────────────────
// 3.  JWT AUTHENTICATION
// ─────────────────────────────────────────────────────────────────────────────
string jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException(
           "JwtSettings:SecretKey is not configured.");

string jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException(
           "JwtSettings:Issuer is not configured.");

string jwtAudience = builder.Configuration["JwtSettings:Audience"]
    ?? throw new InvalidOperationException(
           "JwtSettings:Audience is not configured.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ─────────────────────────────────────────────────────────────────────────────
// 4.  DEPENDENCY INJECTION
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ─────────────────────────────────────────────────────────────────────────────
// 5.  CORS
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ─────────────────────────────────────────────────────────────────────────────
// 6.  SWAGGER
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ShopEZ – User Service",
        Version = "v1"
    });
    options.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter: Bearer {token}"
        });
    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id   = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

// ─────────────────────────────────────────────────────────────────────────────
// 7.  HEALTH CHECK
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<UserDbContext>();

var app = builder.Build();

// ─────────────────────────────────────────────────────────────────────────────
// 8.  AUTO-APPLY MIGRATIONS
// ─────────────────────────────────────────────────────────────────────────────
using (IServiceScope scope = app.Services.CreateScope())
{
    UserDbContext db = scope.ServiceProvider
        .GetRequiredService<UserDbContext>();
    db.Database.Migrate();
}

// ─────────────────────────────────────────────────────────────────────────────
// 9.  MIDDLEWARE PIPELINE
// ─────────────────────────────────────────────────────────────────────────────
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService v1"));
//}

app.UseSwagger();
app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService v1"));

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();