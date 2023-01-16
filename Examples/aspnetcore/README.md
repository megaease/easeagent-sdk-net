# Easeagent SDK .Net example
This is an example app where three .NET services use [easeagent-sdk-net](https://github.com/megaease/easeagent-sdk-net).

## Requirements

In order to build the example, you need to install a tow things:

- [Dotnet](https://dotnet.microsoft.com/)
- [Docker](https://docs.docker.com/engine/installation/) (optional, if you use Megaease Cloud this is not needed)

## Running the example

This example has two services: frontend and backend. They both report trace data to zipkin.

To setup the demo, do

```bash
$ pwd
~/easeagent-sdk-net/
$ ./source.sh
```

Once the dependencies are installed, run the services:

```bash
# Run zipkin (optional):
docker run -p 9411:9411 -d openzipkin/zipkin

# In terminal 1:
$ pwd
~/easeagent-sdk-net/Examples/aspnetcore/backend
$ EASEAGENT_CONFIG=`pwd`/agent_backend.yml dotnet run

# In terminal 2
$ pwd
~/easeagent-sdk-net/Examples/aspnetcore/frontend
$ EASEAGENT_CONFIG=`pwd`/agent_frontend.yml dotnet run

```

And then, request the frontend:
 
```
curl http://localhost:8081
```

1. This starts a trace in the frontend (http://localhost:8081/)
2. Continues the trace and calls the backend (http://localhost:9000)
3. Next, you can view traces that went through the backend via http://localhost:9411/?serviceName=demo.net.frontend_service.


## Running example with a MegaEase Cloud location:

If you need to pass the MegaEase Cloud endpoint, please download the `agent.yml` from [MegaEase Cloud](https://cloud.megaease.com/). By [Details](https://github.com/megaease/easeagent-sdk-net/blob/main/doc/megaease-cloud-config.md#third-about-megaease-cloud).

`MEGAEASE_CLOUD_URL` and `TLS` will be filled in for you automatically.

Replace `agent.yml` with the configuration file in the `configs` directory. Then run it.