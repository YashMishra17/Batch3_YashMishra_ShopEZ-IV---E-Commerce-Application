//using Ocelot.DependencyInjection;
//using Ocelot.Middleware;

//var builder = WebApplication.CreateBuilder(args);

//// ─────────────────────────────────────────────────────────────────────────────
//// 1.  CONFIGURATION
////     Loads appsettings.json then the environment-specific ocelot file.
////     Development  → ocelot.Development.json  (localhost ports)
////     Production   → ocelot.json              (Docker service names)
//// ─────────────────────────────────────────────────────────────────────────────
//builder.Configuration
//    .SetBasePath(builder.Environment.ContentRootPath)
//    .AddJsonFile("appsettings.json",
//                 optional: false,
//                 reloadOnChange: true)
//    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
//                 optional: true,
//                 reloadOnChange: true)
//    .AddJsonFile("ocelot.json",
//                 optional: false,
//                 reloadOnChange: true)
//    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json",
//                 optional: true,
//                 reloadOnChange: true)
//    .AddEnvironmentVariables();

//// ─────────────────────────────────────────────────────────────────────────────
//// 2.  CORS
////     Angular SPA on http://localhost:4200
////     AllowCredentials is required so the browser sends the JWT Authorization header
//// ─────────────────────────────────────────────────────────────────────────────
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAngular", policy =>
//        policy
//            .WithOrigins(
//                "http://localhost:4200",
//                "https://localhost:4200")
//            .AllowAnyMethod()
//            .AllowAnyHeader()
//            .AllowCredentials());
//});

//// ─────────────────────────────────────────────────────────────────────────────
//// 3.  OCELOT
//// ─────────────────────────────────────────────────────────────────────────────
//builder.Services.AddOcelot(builder.Configuration);

//// ─────────────────────────────────────────────────────────────────────────────
//// 4.  HEALTH CHECK  — used by docker-compose depends_on healthcheck
//// ─────────────────────────────────────────────────────────────────────────────
//builder.Services.AddHealthChecks();

//var app = builder.Build();

//// ─────────────────────────────────────────────────────────────────────────────
//// 5.  PIPELINE
////     CORS must be registered BEFORE Ocelot
////     Ocelot must be the LAST middleware
//// ─────────────────────────────────────────────────────────────────────────────
//app.UseCors("AllowAngular");

//app.MapHealthChecks("/health");

//await app.UseOcelot();

//app.Run();

//////////////////////////////////////////////////////////////////////////////////////////

using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true, reloadOnChange: true)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json",
                 optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy
            .WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors("AllowAngular");

app.MapHealthChecks("/health");

await app.UseOcelot();

app.Run();