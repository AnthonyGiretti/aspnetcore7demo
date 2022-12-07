using gRPCDotnet7.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Demo Transcoding
builder.Services.AddGrpc().AddJsonTranscoding();

// Demo Healthcheck
builder.Services.AddGrpcHealthChecks()
    .AddCheck("Health", () => HealthCheckResult.Healthy());

builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.Zero;
    options.Period = TimeSpan.FromSeconds(3);
});

var app = builder.Build();

// Demo Healthcheck
app.MapGrpcHealthChecksService();

app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
