using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseSetting("urls", "http://localhost:9000")
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
