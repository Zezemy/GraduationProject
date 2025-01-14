using Signalizer.Context;
using Signalizer.Hubs;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Signalizer.BackgroundServices;
using Signalizer.Managers;
using Signalizer.Entities.BackgroundServices;
//using Signalizer.Migrations;
//using Signalizer.Repositories;
//using Signalizer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Signalizer.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
// Add services to the container.
builder.Services.AddProblemDetails();


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
}));

builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

builder.Services.AddBinance();

builder.Services.AddSingleton<ActiveTickerManager>();
builder.Services.AddHostedService<PriceFeedUpdater>();

builder.Services.AddHostedService<MaCrossoverSignaller>();
builder.Services.AddHostedService<RsiSignaller>();
builder.Services.AddHostedService<MacdSignaller>();
builder.Services.AddHostedService<BollingerBandsSignaller>();
builder.Services.AddHostedService<StochasticOscillatorSignaller>();
builder.Services.AddHostedService<TripleMaCrossoverSignaller>();
builder.Services.AddHostedService<PriceChannelSignaller>();
builder.Services.AddHostedService<VolumePriceTrendSignaller>();
builder.Services.AddHostedService<MomentumSignaller>();
builder.Services.AddHostedService<ExponentialMaCrossoverWithVolumeSignaller>();

builder.Services.AddHostedService<TradingSignaller>();

builder.Services.Configure<UpdateOptions>(builder.Configuration.GetSection("PriceUpdateOptions"));

builder.Services.AddAuthorization();
// add identity and opt-in to endpoints
builder.Services.AddIdentityCore<User>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddRuntimeInstrumentation()
            .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel", "System.Net.Http");
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    });

var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
if (useOtlpExporter)
{
    builder.Services.AddOpenTelemetry().UseOtlpExporter();
}
//builder.Services.ConfigureApplicationCookie(options => {
//    options.Cookie.SameSite = SameSiteMode.None;
//    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
//});
var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        //db.Database.Migrate();
        await SeedData.InitializeAsync(scope.ServiceProvider);
    }
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<PriceFeedHub>("/pricehub");
app.MapHub<TradingSignalSenderHub>("/trading-signal-sender-hub");

app.MapIdentityApi<User>();
//app.UseAntiforgery();

app.MapPost("/logout", async (SignInManager<User> signInManager, [FromBody] object empty) =>
{
    if (empty is not null)
    {
        await signInManager.SignOutAsync();

        return Results.Ok();
    }

    return Results.Unauthorized();
}).RequireAuthorization();


// provide an endpoint for user roles
app.MapGet("/roles", (ClaimsPrincipal user) =>
{
    if (user.Identity is not null && user.Identity.IsAuthenticated)
    {
        var identity = (ClaimsIdentity)user.Identity;
        var roles = identity.FindAll(identity.RoleClaimType)
            .Select(c =>
                new
                {
                    c.Issuer,
                    c.OriginalIssuer,
                    c.Type,
                    c.Value,
                    c.ValueType
                });

        return TypedResults.Json(roles);
    }

    return Results.Unauthorized();
}).RequireAuthorization();

app.UseResponseCompression();

app.UseCors("corsapp");

app.Run();
