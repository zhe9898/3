using Zongzu.Host;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Lock the listen endpoint to loopback so Windows firewall stays silent and
// no external process can reach the authoritative simulation.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5173);
});

builder.Services.AddSingleton<SimulationHostService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    // PascalCase so JSON field names match C# property names exactly.
    // Unity's Newtonsoft.Json default also expects PascalCase, keeping
    // the wire format aligned with the shared view-model source.
    options.SerializerOptions.PropertyNamingPolicy = null;
});

WebApplication app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/hall", (SimulationHostService sim) => Results.Ok(sim.SnapshotShell()));

app.MapPost("/advance-month", (SimulationHostService sim) => Results.Ok(sim.AdvanceOneMonth()));

app.Run();

