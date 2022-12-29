using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using easeagent;
using System.Diagnostics;
using System.IO;

namespace easeagent.UTest
{
    [TestFixture]
    internal class T_Spec
    {
        [Test]
        public void NewOne()
        {
            Spec spec = Spec.NewOne();
            Assert.AreEqual("zone.damoin.service", spec.ServiceName);
            Assert.AreEqual("log-tracing", spec.TracingType);
            Assert.AreEqual(true, spec.TracingEnable);
            Assert.AreEqual(1.0F, spec.SampleRate);
            Assert.AreEqual(true, spec.SharedSpans);
            Assert.AreEqual(false, spec.Id128bit);
            Assert.AreEqual("", spec.OutputServerUrl);
            Assert.AreEqual(false, spec.EnableTls);
            Assert.AreEqual("", spec.TlsKey);
            Assert.AreEqual("", spec.TlsCert);
        }

        [Test]
        public void Load()
        {
            string path = getBasePath("agent_test_normal.yml");
            Spec spec = Spec.Load(path);
            Assert.AreEqual("demo.demo.sdk-php-router-service", spec.ServiceName);
            Assert.AreEqual("log-tracing", spec.TracingType);
            Assert.AreEqual(true, spec.TracingEnable);
            Assert.AreEqual(0.5, spec.SampleRate);
            Assert.AreEqual(true, spec.SharedSpans);
            Assert.AreEqual(false, spec.Id128bit);
            Assert.AreEqual("http://localhost:9411/api/v2/spans", spec.OutputServerUrl);
            Assert.AreEqual(true, spec.EnableTls);
            // \Easeagent\Log\Log::getLogger()->addInfo(spec.tlsKey);
            Assert.AreEqual("----------- key -----------\nkey content\n----------- key end -----------\n", spec.TlsKey);
            Assert.AreEqual("----------- cert -----------\ncert content\n----------- cert end -----------\n", spec.TlsCert);

            spec = Spec.Load(getBasePath("agent_test_empty_tls.yml"));
            Assert.AreEqual("demo.demo.sdk-php-router-service", spec.ServiceName);

            spec = Spec.Load(getBasePath("agent_test_just_service_name.yml"));
            Assert.AreEqual("demo.demo.sdk-php-router-service", spec.ServiceName);
            Assert.AreEqual("log-tracing", spec.TracingType);
            Assert.AreEqual(true, spec.TracingEnable);
            Assert.AreEqual(1.0, spec.SampleRate);
            Assert.AreEqual(true, spec.SharedSpans);
            Assert.AreEqual(false, spec.Id128bit);
            Assert.AreEqual("", spec.OutputServerUrl);
            Assert.AreEqual(false, spec.EnableTls);
            Assert.AreEqual("", spec.TlsKey);
            Assert.AreEqual("", spec.TlsCert);

        }

        public static string getBasePath(string fileName)
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            int index = path.IndexOf("Tests");
            return path.Substring(0, index + "Tests".Length) + Path.DirectorySeparatorChar + fileName;
        }
    }
}