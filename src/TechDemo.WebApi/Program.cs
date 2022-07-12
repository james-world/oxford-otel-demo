using System;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry;
using System.Diagnostics.Metrics;

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
        .AddSource("TechDemo.WebApi")
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(opt => {
            opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }
    );
});

const string customMeterName = "MyMeter";
var meter = new Meter(customMeterName);
var counter = meter.CreateCounter<int>("my-counter");
builder.Services.AddOpenTelemetryMetrics(metricsBuilder =>
{
    metricsBuilder
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddMeter(customMeterName)
        .AddOtlpExporter(opt => {
            opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        });
});



var app = builder.Build();

var tracer = app.Services.GetService<TracerProvider>()!.GetTracer("TechDemo.WebApi");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/", async ([FromServices]ILogger<Program> logger) => {
    logger.LogInformation("Root endpoint called: Hello World!");
   
    await Foo();

    return Results.Ok(new {
        Message = "Hello World!"
    });
});
        
app.Run();

// A function that does nothing but wait for a bit to demonatrate child spans
async Task Foo()
{
    Counters.Requests++;

    using (var span = tracer.StartActiveSpan("foo")) // create a child span
    {
        span.AddEvent("Doing some work...");
        
        await Task.Delay(TimeSpan.FromMilliseconds(10))
            .ConfigureAwait(false);

        try {
            if (Counters.Requests == 3) {
                throw new Exception("Something went wrong");
            }
            else
            {
            
                span.AddEvent("Did it!"); // attach an event to the span
                span.SetStatus(Status.Ok); // set the status of the span
            }
        }
        catch (Exception ex)
        {
            span.AddEvent("Something went wrong");
            span.SetStatus(Status.Error);
            span.RecordException(ex);
            throw;
        }
        
        counter.Add(1); // update a metric
    }
}

static class Counters
{
    public static int Requests = 0;
}