using System;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var attributes = new Dictionary<string, object>
{
    ["host.name"] = Environment.MachineName,
    ["os.description"] = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
    ["deployment.environment"] = builder.Environment.EnvironmentName.ToLowerInvariant()
};
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("TechDemo.WebApi", serviceVersion: "1.0.0", serviceNamespace: "TechDemo")
    .AddTelemetrySdk()
    .AddAttributes(attributes);


builder.Services.AddLogging(loggingBuilder => {
    loggingBuilder.ClearProviders();
    loggingBuilder.AddJsonConsole();
    loggingBuilder.AddOpenTelemetry(logOpt => {    
        logOpt
            .SetResourceBuilder(resourceBuilder)
            .AddOtlpExporter(opt => {        
               opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
           });
    });
});

builder.Services.AddOpenTelemetryTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(opt => {
            opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }
    );
});

builder.Services.AddOpenTelemetryMetrics(metricsBuilder =>
{
    metricsBuilder
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(opt => {
            opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/", ([FromServices]ILogger<Program> logger) => {
    logger.LogInformation("Root endpoint called: Hello World!");
    return Results.Ok(new {
        Message = "Hello World!"
    });
});
        
app.Run();