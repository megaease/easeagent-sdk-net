# easeagent-sdk-net

First ***production ready***, simple and full Easeagent SDK implementation without dependencies.

## Installing via dotnet

easeagent-sdk-net can be installed package:
```bash
$ dotnet add package easeagent --version 0.0.1
```

## Usage
### First: Configuration
create a yaml file config for your server like this: [agent.yml](./agent.yml)

If you are using `MegaEase Cloud`. Please download the agent.yml on the front end. `YOUR_SERVCIE_NAME`,`TYPE_OF_TRACING`,`MEGAEASE_CLOUD_URL` and `TLS` will be filled in for you automatically.

### Second: Register Agent

```csharp
easeagent.Agent.RegisterFromYaml(Environment.GetEnvironmentVariable("EASEAGENT_CONFIG"));

//Run your application

//On shutdown
easeagent.Agent.Stop();
```

Your zipkin client is now ready!


### Third: use HTTP Server Transaction and Client Handler for span

##### 1. Server Transaction

```csharp
using zipkin4net.Middleware;
app.UseTracing(easeagent.Agent.GetServiceName());
```

##### 2. Client 
```csharp
using zipkin4net.Transport.Http;

// init client before
services.AddHttpClient("Tracer").AddHttpMessageHandler(provider =>
    TracingHandler.WithoutInnerHandler(easeagent.Agent.GetServiceName()));

// use httpclient after.
var callServiceUrl = config["callServiceUrl"];
var clientFactory = app.ApplicationServices.GetService<IHttpClientFactory>();
using (var httpClient = clientFactory.CreateClient("Tracer"))
{
    var response = await httpClient.GetAsync(callServiceUrl);
}
```

### Play with traces

To create a new trace, simply call

```csharp
var trace = Trace.Create();
```

Then, you can record annotations

```csharp
trace.Record(Annotations.ServerRecv());
trace.Record(Annotations.ServiceName(serviceName));
trace.Record(Annotations.Rpc("GET"));
trace.Record(Annotations.ServerSend());
trace.Record(Annotations.Tag("http.url", "<url>")); //adds binary annotation
```

We provide an interface so that you can decorate the Span of the middleware, please refer to another [document](./megaease-cloud-config.md) for the reason of decoration.
```csharp
// decorate a span to rabbitmq
Agent.RecordMiddleware(trace, easeagent.Middleware.Type.RabbitMQ);
```

