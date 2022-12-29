using System.Net;

namespace easeagent.Tracers.Zipkin.V2
{
    public class Endpoint
    {
        public readonly string ServiceName;
        public readonly IPEndPoint HostPort;

        public Endpoint(string serviceName, IPEndPoint hostPort)
        {
            ServiceName = serviceName;
            HostPort = hostPort;
        }
    }
}