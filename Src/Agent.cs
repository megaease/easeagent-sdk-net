using System;
using easeagent.Log;
using easeagent.Tracers.Zipkin;
using easeagent.Transport.Http;
using zipkin4net;
using zipkin4net.Tracers.Zipkin;

namespace easeagent
{
    public class Agent
    {
        //Environment.GetEnvironmentVariable("EASEAGENT_CONFIG")
        public static void RegisterFromYaml(String yamlFile)
        {
            Spec spec = Spec.Load(yamlFile);
            spec.Validate();
            if (spec.TracingEnable)
            {
                TraceManager.SamplingRate = spec.SampleRate;
            }
            else
            {
                TraceManager.SamplingRate = 0;
            }
            TraceManager.Trace128Bits = spec.Id128bit;
            IZipkinSender sender = default(IZipkinSender);
            if (spec.OutputServerUrl == null || spec.OutputServerUrl.Trim().Equals(""))
            {
                LoggerManager.GetTracingLogger().LogInformation("out put server was empty, use console reporter for trace.");
                sender = new ConsoleSender();
            }
            else
            {
                sender = HttpSender.Build(spec);
            }
            var serializer = new JSONSpanSerializerV2(spec.ServiceName, spec.TracingType);
            var tracer = new ZipkinTracer(sender, serializer);
            TraceManager.RegisterTracer(tracer);
            TraceManager.Start(LoggerManager.GetTracingLogger());
        }

        public static void Stop()
        {
            TraceManager.Stop();
        }
    }
}