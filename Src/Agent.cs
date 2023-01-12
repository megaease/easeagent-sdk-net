using System;
using System.Collections.Generic;
using easeagent.Log;
using easeagent.Tracers.Zipkin;
using easeagent.Transport.Http;
using zipkin4net;
using zipkin4net.Propagation;
using zipkin4net.Tracers.Zipkin;
using easeagent.Middleware;

namespace easeagent
{
    public delegate string HeaderGetter(string key);
    public class Agent
    {
        private static Spec spec = Spec.NewOne();
        private static zipkin4net.Propagation.IInjector<Dictionary<string, string>> injector = Propagations.B3String.Injector<Dictionary<string, string>>((carrier, key, value) =>
        {
            carrier.Add(key, value);
        });

        private static zipkin4net.Propagation.IExtractor<HeaderGetter> extractor = Propagations.B3String.Extractor<HeaderGetter>((carrier, key) =>
        {
            return carrier(key);

        });


        //Environment.GetEnvironmentVariable("EASEAGENT_CONFIG")
        public static void RegisterFromYaml(String yamlFile)
        {
            spec = Spec.Load(yamlFile);
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
                try
                {
                    sender = HttpSender.Build(spec);
                }
                catch (Exception e)
                {
                    LoggerManager.GetTracingLogger().LogError("build http sender fail, use console reporter for trace: " + e.ToString());
                    sender = new ConsoleSender();

                }
            }
            var serializer = new JSONSpanSerializerV2(spec.ServiceName, spec.TracingType);
            var tracer = new ZipkinTracer(sender, serializer);
            TraceManager.RegisterTracer(tracer);
            TraceManager.Start(LoggerManager.GetTracingLogger());
        }

        public static string GetServiceName()
        {
            return spec.ServiceName;
        }

        public static void Stop()
        {
            TraceManager.Stop();
        }

        public static Dictionary<string, string> InjectToDict(Trace trace)
        {
            var result = new Dictionary<string, string>();
            injector.Inject(trace.CurrentSpan, result);
            return result;
        }

        public static Trace ExtractToTrace(HeaderGetter getter)
        {
            var context = extractor.Extract(getter);
            return Trace.CreateFromId(context);
        }

        public static void RecordMiddleware(Trace trace, easeagent.Middleware.Type type)
        {
            trace.Record(Annotations.Tag(easeagent.Middleware.TypeExtensions.TAG, type.value()));
        }
    }
}