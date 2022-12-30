using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using easeagent.Log;
using zipkin4net.Tracers.Zipkin;

namespace easeagent.Transport.Http
{
    public class HttpSender : IZipkinSender
    {
        private const string _contentType = "application/json";
        private string _collectorUrl;
        private HttpClient _httpClient;


        public HttpSender(HttpClient httpClient, string collectorUrl)
        {
            _httpClient = httpClient;
            _collectorUrl = collectorUrl;
        }

        public static HttpSender Build(Spec spec)
        {
            HttpClient client = default(HttpClient);
            if (!spec.EnableTls)
            {
                client = new HttpClient();
            }
            else
            {
                client = new HttpClient(new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    SslProtocols = SslProtocols.Tls12,
                    ClientCertificates = { buildX509Certificate2(spec.TlsKey, spec.TlsCert) }
                });
            }
            return new HttpSender(client, spec.OutputServerUrl);
        }

        private static X509Certificate2 buildX509Certificate2(string key, string cert)
        {
            using (X509Certificate2 pubOnly = new X509Certificate2(System.Text.Encoding.ASCII.GetBytes(cert)))
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportFromPem(key);

                using (X509Certificate2 pubPrivEphemeral = pubOnly.CopyWithPrivateKey(rsa))
                {
                    // Export as PFX and re-import if you want "normal PFX private key lifetime"
                    // (this step is currently required for SslStream, but not for most other things
                    // using certificates)
                    return new X509Certificate2(pubPrivEphemeral.Export(X509ContentType.Pfx));
                }
                // do stuff with the key now
            }
        }

        public void Send(byte[] data)
        {
            var content = new ByteArrayContent(data);
            content.Headers.Add("Content-Type", _contentType);
            content.Headers.Add("Content-Length", data.Length.ToString());
            content.Headers.Add("b3", "0");
            Task<HttpResponseMessage> task = _httpClient.PostAsync(_collectorUrl, content);
            task.ContinueWith((task, o) =>
            {
                TaskAwaiter<HttpResponseMessage> a = task.GetAwaiter();
                try
                {
                    HttpResponseMessage result = a.GetResult();
                    LoggerManager.GetTracingLogger().LogInformation(result.ToString());
                    if (!result.IsSuccessStatusCode)
                    {
                        LoggerManager.GetTracingLogger().LogWarning("send data to collector fail: " + result.ToString());
                    }

                }
                catch (Exception e)
                {
                    LoggerManager.GetTracingLogger().LogError($"send data to {_collectorUrl} faild: {e.ToString()}");
                }
            }, null);
        }
    }
}