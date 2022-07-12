# Open Telemetry Demo

## Running the code

- Run `.\build` from a powershell / bash prompt to build.
- Run `docker compose up` (docker required!) or `.\run-otel.ps1` to start an Open Telemetry Collection and various Backends
- Start the API project with `dotnet run --project src/TechDemo.WebApi` or `.\run-api.ps1`
- Run the client to send in a call with `dotnet run --project src/TechDemo.Client` or `.\run-client.ps1`
    - The client will call the server, and a trace will be logged across the call consisting of three spans. The inner span 'foo' will have some events recorded. A log entry will be recorded by the server. A metric called 'my-counter' will be incremented. The third time the API is called, it will thrown an exception, and record that to the trace.
- When you are done, use CTRL+C to stop docker compose, and run `docker compose down` to dispose of all resources.

Browse the various backends to look at the data:

- Grafana at `http://localhost:3000` - here you can explore the Loki and Prometheus data for logs and metrics.
- Jaegar at `http://localhost:16686` - here you can explore traces.
- Zipkin at `http://localhost:9411` - here you can explore an alternative tracing backennd.

## Guide to configuration files

- `docker-compose.yaml` contains the docker stack for the otel collector and all the backends
- `prometheus.yaml` contains the config for prometheus which consists of scape jobs to pull metrics from the collector
- `grafana.ini` contains config for grafana that turns off authentication for easy demoing
- `datasource.yaml` contains config for grafana which consists of the loki and prometheus data sources
- `.env` contains some environment variables which are passed to the otel collector startup cmd in the docker compose
- `otel-collector-config.yaml` contains the configuration of the collector, showing how all the backends are wired up

Find out all about Open Telemetry [here](https://opentelemetry.io)

A few references used for config:

- Building the docker-compose for otel-collector, jaegar, zipkin and prometheus: https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/examples/demo
- Adding grafana and loki:
- https://raw.githubusercontent.com/grafana/loki/v2.6.0/production/docker-compose.yaml
- https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/exporter/lokiexporter/README.md

Loki doesn't like labels with '.' in so setting up renames in otel-collector-config.yaml is important. Use otel console logging in
.NET to see the attributes and resources being written that need fixing. If any are bad, the whole log message is dropped.

Prometheus needs properly named labels (attributes and metrics) too. A normalizer exists for the otel collector prometheus
exporter as a preview feature and must be
enabled for metrics sent by .NET. See https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/pkg/translator/prometheus. The .env file contains the feature flag needed to turn normalization on, and this is passed in the docker-compose file
to the command used to start the otel collector.

## Telemetry in .NET

See https://github.com/open-telemetry/opentelemetry-dotnet for general configuration.
See https://opentelemetry.io/docs/instrumentation/net/getting-started/ for a good getting started guide.

## Backend Docs

- Loki: https://grafana.com/docs/loki/latest/
- Grafana: https://grafana.com/docs/grafana/latest/?pg=oss-graf&plcmt=quick-links
- Promethus: https://prometheus.io/docs/introduction/overview/
- Zipkin: https://zipkin.io/
- Jaegar: https://www.jaegertracing.io/docs/1.36/getting-started/