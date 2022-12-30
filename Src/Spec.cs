using System;
using System.Collections.Generic;
using System.IO;
using easeagent.Log;
using YamlDotNet.Serialization.NamingConventions;

namespace easeagent
{
    public class Spec
    {
        public string OutputServerUrl { get; set; }
        public bool EnableTls { get; set; }
        public string TlsKey { get; set; }
        public string TlsCert { get; set; }
        public string ServiceName { get; set; }
        public string TracingType { get; set; }

        public bool TracingEnable { get; set; }
        public float SampleRate { get; set; }
        public bool SharedSpans { get; set; }
        public bool Id128bit { get; set; }

        public static Spec NewOne()
        {
            return new Spec
            {
                ServiceName = "zone.damoin.service",
                TracingType = "log-tracing",
                TracingEnable = true,
                SampleRate = 1.0F,
                SharedSpans = true,
                Id128bit = false,
                OutputServerUrl = "",  // empty output is logger span json
                EnableTls = false,
                TlsKey = "",
                TlsCert = ""
            };
        }


        public static Spec Load(string yamlFile)
        {
            Spec spec = NewOne();
            if (yamlFile == null)
            {
                LoggerManager.GetTracingLogger().LogInformation("yamlFile was ''");
                return spec;
            }
            IDictionary<string, string> dict = default(IDictionary<string, string>);
            try
            {
                var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
                dict = deserializer.Deserialize<IDictionary<string, string>>(File.ReadAllText(yamlFile));
                LoggerManager.GetTracingLogger().LogInformation("load spec from yaml: " + yamlFile);
            }
            catch (Exception e)
            {
                LoggerManager.GetTracingLogger().LogError("read yaml file:" + yamlFile + " fail, use default logger span json for:" + e.ToString());
                return spec;
            }


            foreach (KeyValuePair<string, string> kvp in dict)
            {
                switch (kvp.Key)
                {
                    case "serviceName":
                        spec.ServiceName = (string)(isEmpty(kvp.Value) ? "" : kvp.Value);
                        break;
                    case "tracing.type":
                        spec.TracingType = (string)(isEmpty(kvp.Value) ? "" : kvp.Value);
                        break;
                    case "tracing.enable":
                        spec.TracingEnable = isEmpty(kvp.Value) ? true : bool.Parse(kvp.Value);
                        break;
                    case "tracing.sample.rate":
                        spec.SampleRate = isEmpty(kvp.Value) ? 1.0F : float.Parse(kvp.Value);
                        break;
                    case "tracing.shared.spans":
                        spec.SharedSpans = isEmpty(kvp.Value) ? true : bool.Parse(kvp.Value);
                        break;
                    case "tracing.id128bit":
                        spec.Id128bit = isEmpty(kvp.Value) ? false : bool.Parse(kvp.Value);
                        break;
                    case "reporter.output.server":
                        spec.OutputServerUrl = isEmpty(kvp.Value) ? "" : kvp.Value;
                        break;
                    case "reporter.output.server.tls.enable":
                        spec.EnableTls = isEmpty(kvp.Value) ? false : bool.Parse(kvp.Value);
                        break;
                    case "reporter.output.server.tls.key":
                        spec.TlsKey = isEmpty(kvp.Value) ? "" : kvp.Value;
                        break;
                    case "reporter.output.server.tls.cert":
                        spec.TlsCert = isEmpty(kvp.Value) ? "" : kvp.Value;
                        break;
                    default:
                        break;
                }
            }
            return spec;
        }

        public static bool isEmpty(string v)
        {
            return v == null || v.Length == 0 || v.Trim().Length == 0;
        }

        public void Validate()
        {
            if (EnableTls && (isEmpty(TlsKey) || isEmpty(TlsCert)))
            {
                throw new FormatException("key, cert are not all specified");
            }
        }

    }
}