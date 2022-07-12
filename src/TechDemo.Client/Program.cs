using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var attributes = new Dictionary<string, object>
{
    ["host.name"] = Environment.MachineName,
    ["os.description"] = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
};
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("TechDemo.Client", serviceVersion: "1.0.0", serviceNamespace: "TechDemo")
    .AddTelemetrySdk()
    .AddAttributes(attributes);

// The using here will dispose the tracer object which
// ensures that traces are flushed to the backend before quitting.
using var tracer = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddHttpClientInstrumentation()
    .AddOtlpExporter(opt => {
        opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    })
    .Build();

using var client = new HttpClient();

var response = await client.GetAsync("http://localhost:5201");
