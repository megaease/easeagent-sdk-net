# easeagent-sdk-net

First ***production ready***, simple and full Easeagent SDK implementation without dependencies.

## Installing via Composer

easeagent-sdk-net can be installed package:
```bash
$ dotnet add package zipkin4net --version 0.0.1
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

### Use for package

Zipkin provides some official packages, you can also use them easily, such as:
```csharp
// init client before
services.AddHttpClient("Tracer").AddHttpMessageHandler(provider =>
    TracingHandler.WithoutInnerHandler(provider.GetService<IConfiguration>()["applicationName"]));


// use httpclient after.
var callServiceUrl = config["callServiceUrl"];
var clientFactory = app.ApplicationServices.GetService<IHttpClientFactory>();
using (var httpClient = clientFactory.CreateClient("Tracer"))
{
    var response = await httpClient.GetAsync(callServiceUrl);
}

```
