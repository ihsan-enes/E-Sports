using System.Net.Http.Headers;
using ESpor.Api.Middleware;
using ESpor.Application.Common.Interfaces;
using ESpor.Application.Common.Options;
using ESpor.Application.External.Interfaces;
using ESpor.Application.Games.Handlers;
using ESpor.Application.Sync.Handlers;
using ESpor.Application.Sync.Interfaces;
using ESpor.Application.Sync.Services;
using ESpor.Infrastructure.External.PandaScore;
using ESpor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Database Context
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? builder.Configuration["ConnectionStrings__Default"]
    ?? throw new InvalidOperationException("Bağlantı dizesi 'Default' bulunamadı.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
    });
    options.UseSnakeCaseNamingConvention();
});

builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

// Configuration Options
builder.Services.Configure<RelevanceOptions>(
    builder.Configuration.GetSection(RelevanceOptions.SectionName));

// PandaScore Typed HttpClient + Standard Resilience Handler
var pandaScoreApiKey = builder.Configuration["PandaScore:ApiKey"]
    ?? builder.Configuration["PandaScore__ApiKey"]
    ?? string.Empty;

builder.Services.AddHttpClient<IEsportsDataProvider, PandaScoreProvider>(client =>
{
    client.BaseAddress = new Uri("https://api.pandascore.co/");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    if (!string.IsNullOrWhiteSpace(pandaScoreApiKey))
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", pandaScoreApiKey);
    }
})
.AddStandardResilienceHandler();

// Sync Service & Handlers
builder.Services.AddScoped<IEsportsSyncService, EsportsSyncService>();
builder.Services.AddScoped<TriggerSyncHandler>();
builder.Services.AddScoped<GetGamesHandler>();

// Controllers & Global Exception Handling
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// OpenAPI
builder.Services.AddOpenApi();

// CORS
const string devCorsPolicy = "DevCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(devCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Auto-migrate on startup in Development only
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("E-Spor API");
        options.WithTheme(ScalarTheme.Moon);
    });
}

app.UseExceptionHandler();

app.UseCors(devCorsPolicy);

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
