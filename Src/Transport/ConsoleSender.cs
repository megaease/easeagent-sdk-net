using easeagent.Log;
using zipkin4net;
using zipkin4net.Tracers.Zipkin;

namespace easeagent.Transport.Http
{
    public class ConsoleSender : IZipkinSender
    {
        private ILogger _logger = LoggerManager.GetTracingLogger();
        public void Send(byte[] data)
        {
            _logger.LogInformation(System.Text.Encoding.UTF8.GetString(data, 0, data.Length));
        }
    }
}