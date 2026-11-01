using MediatR;
using RunbookForge.BuildingBlocks.Common.Behaviors;
using RunbookForge.BuildingBlocks.Web.Contracts;
using RunbookForge.Modules.RunbookEngine.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Core Services & ProblemDetails
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RunbookForge API", Version = "v1" });
});
builder.Services.AddHealthChecks();

// 2. Module Infrastructure Services
builder.Services.AddRunbookEngineInfrastructure(builder.Configuration);

// 3. MediatR Registration Across All Module Assemblies & Pipeline Behaviors
var assemblies = new[]
{
    typeof(RunbookForge.BuildingBlocks.Common.Results.Result).Assembly,
    typeof(RunbookForge.Modules.RunbookEngine.Application.Features.Placeholders).Assembly,
    typeof(RunbookForge.Modules.TicketIngestion.Application.Features.Placeholders).Assembly,
    typeof(RunbookForge.Modules.PatternAnalytics.Application.Features.Placeholders).Assembly,
    typeof(RunbookForge.Modules.GapAnalyzer.Application.Features.Placeholders).Assembly,
    typeof(RunbookForge.Modules.AICriticAndTraining.Application.Features.Placeholders).Assembly
};

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assemblies);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// 3. Register Endpoint Modules via Reflection
var endpointModuleType = typeof(IEndpointModule);
var endpointModules = assemblies
    .SelectMany(a => a.GetTypes())
    .Where(t => endpointModuleType.IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
    .Select(Activator.CreateInstance)
    .Cast<IEndpointModule>()
    .ToList();

foreach (var module in endpointModules)
{
    builder.Services.AddSingleton(endpointModuleType, module);
}

var app = builder.Build();

// 4. HTTP Request Pipeline
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RunbookForge API v1"));
}

app.UseHttpsRedirection();

// 5. Map Health Check & Module Endpoints
app.MapHealthChecks("/health");

var mappedModules = app.Services.GetServices<IEndpointModule>();
foreach (var module in mappedModules)
{
    module.MapEndpoints(app);
}

app.MapGet("/", () => Results.Ok(new
{
    System = "RunbookForge",
    Status = "Operational",
    Version = "1.0.0",
    Documentation = "/swagger"
}));

await app.RunAsync();

