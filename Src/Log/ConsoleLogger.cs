using System;

namespace easeagent.Log
{
    public class ConsoleLogger : zipkin4net.ILogger
    {
        public void LogError(string message)
        {
            Console.Error.WriteLine("Easeagent [ERROR] " + message);
        }


        public void LogInformation(string message)
        {
            Console.WriteLine("Easeagent [INFO] " + message);
        }

        public void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Easeagent [WARNING] " + message);
            Console.ResetColor();
        }
    }
}