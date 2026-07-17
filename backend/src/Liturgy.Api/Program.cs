using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Liturgy.Api.Middleware;
using Liturgy.Application;
using Liturgy.Infrastructure;
using Liturgy.Infrastructure.Realtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt section is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // SignalR sends the bearer token as a query-string parameter on the WebSocket
        // handshake, so lift it into the auth context for the hub path.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? new[] { "http://localhost:4200", "https://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("web", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddAuthorization();
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        // Enum names on the wire so the Angular client and backend share one string
        // contract (PhaseKind, BoardColumn, RKind, GateState, ... all round-trip by name).
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // Seed the New Hope Collective demo workspace in every environment. The deployed
    // showcase is built entirely around this data, and sign-in has no value without the
    // demo accounts it creates. SeedAsync is idempotent (it no-ops once a workspace
    // exists), so this is safe to run on every startup.
    var seeder = scope.ServiceProvider.GetRequiredService<DevDataSeeder>();
    await seeder.SeedAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve the static marketing site and the bundled Angular SPA from wwwroot. The CI
// pipeline copies both here before publish (marketing/index.html owns "/", the SPA
// entry is emitted as app.html), so a single App Service hosts marketing, UI, and
// API. In local development wwwroot is empty and the Angular dev server (ng serve)
// is used instead, so these calls are simply no-ops.
// Angular bundles are content-hashed (e.g. main-7X2KQ5RD.js) and safe to cache
// forever; everything unhashed (app.html, index.html, liturgy.css, the PDF) must
// revalidate so deploys take effect immediately.
var hashedAsset = new Regex(@"-[A-Z0-9]{8,}\.[a-z0-9]+$", RegexOptions.IgnoreCase);
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
        ctx.Context.Response.Headers.CacheControl = hashedAsset.IsMatch(ctx.File.Name)
            ? "public, max-age=31536000, immutable"
            : "no-cache",
});

// Route matching must happen AFTER the static-file middleware: WebApplication's
// implicit UseRouting runs first in the pipeline, where the SPA fallback endpoint
// ({*path:nonfile}) would match "/" and stop UseDefaultFiles from rewriting it to
// the marketing index.html.
app.UseRouting();

app.UseCors("web");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();
app.MapHub<BoardHub>("/hubs/board");

// Any request that isn't an API/hub route or an existing static file falls through
// to the SPA entry point so Angular's client-side router can handle deep links
// (e.g. /board/123 on refresh). The entry is app.html — index.html is the
// marketing splash.
app.MapFallbackToFile("app.html");

app.Run();

public partial class Program { }
