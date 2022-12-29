using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using easeagent.Log;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace easeagent.UTest.Log
{
    [TestFixture]
    public class T_LoggerManager
    {
        [Test]
        public void Test1()
        {
            LoggerManager.GetTracingLogger().LogError("--------- error ----------");
            LoggerManager.GetTracingLogger().LogWarning("----------- warning --------");
            LoggerManager.GetTracingLogger().LogInformation("--------- info ----------");
        }
    }
}