
namespace easeagent.Log
{
    public class LoggerManager
    {
        private static zipkin4net.ILogger _logger = new ConsoleLogger();


        public static zipkin4net.ILogger GetTracingLogger()
        {
            return _logger;
        }

    }
}