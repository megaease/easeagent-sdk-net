using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using zipkin4net.Tracers.Zipkin;
using System.Collections;
using zipkin4net;
using zipkin4net.Utils;
using System.Text;

namespace easeagent.Tracers.Zipkin.V2
{
    //converter from https://github.com/openzipkin/zipkin/blob/master/zipkin/src/main/java/zipkin2/v1/V1SpanConverter.java
    public class V1SpanConverter
    {
        private Span.Builder first = Span.NewBuilder();
        private List<Span.Builder> spans = new List<Span.Builder>();
        private ZipkinAnnotation cs, sr, ss, cr, ms, mr, ws, wr;

        public static V1SpanConverter Create()
        {
            return new V1SpanConverter();
        }


        public List<Span> Convert(zipkin4net.Tracers.Zipkin.Span source)
        {
            List<Span> results = new List<Span>();
            convert(source, results);
            return results;
        }

        private void convert(zipkin4net.Tracers.Zipkin.Span source, List<Span> sink)
        {
            start(source);
            // add annotations unless they are "core"
            processAnnotations(source);
            // convert binary annotations to tags and addresses
            processBinaryAnnotations(source);
            finish(sink);
        }


        private void processAnnotations(zipkin4net.Tracers.Zipkin.Span source)
        {
            Endpoint sourceEndpoint = endpoint(source);
            foreach (ZipkinAnnotation a in source.Annotations)
            {
                forAnnotation(source, a);
            }

            // When bridging between event and span model, you can end up missing a start annotation
            if (cs == null && endTimestampReflectsSpanDuration(cr, source))
            {
                cs = new ZipkinAnnotation(source.SpanStarted.Value, "cs");
            }
            if (sr == null && endTimestampReflectsSpanDuration(ss, source))
            {
                sr = new ZipkinAnnotation(source.SpanStarted.Value, "sr");
            }

            if (cs != null && sr != null)
            {
                // in a shared span, the client side owns span duration by annotations or explicit timestamp
                maybeTimestampDuration(source, cs, cr);

                // special-case loopback: We need to make sure on loopback there are two span2s
                Span.Builder client = forAnnotation(source, cs);
                client.Kind(Span.SpanKind.CLIENT);
                // fork a new span for the server side
                Span.Builder server = newSpanBuilder(source).Kind(Span.SpanKind.SERVER);

                // the server side is smaller than that, we have to read annotations to find out
                server.Shared(true).Timestamp(toMicroseconds(sr.Timestamp));
                if (ss != null) server.Duration(toMicroseconds(ss.Timestamp) - toMicroseconds(sr.Timestamp));
                if (cr == null && !source.Duration.HasValue) client.Duration(0); // one-way has no duration
            }
            else if (cs != null && cr != null)
            {
                maybeTimestampDuration(source, cs, cr);
            }
            else if (sr != null && ss != null)
            {
                maybeTimestampDuration(source, sr, ss);
            }
            else
            { // otherwise, the span is incomplete. revert special-casing
                handleIncompleteRpc(source);
            }

            // Span v1 format did not have a shared flag. By convention, span.timestamp being absent
            // implied shared. When we only see the server-side, carry this signal over.
            if (cs == null && sr != null &&
              // We use a signal of either the authoritative timestamp being unset, or the duration is unset
              // eventhough we have the server send result. The latter clarifies an edge case in MySQL
              // where a span row is shared between client and server. The presence of timestamp in this
              // case could be due to the client-side of that RPC.
              (!source.SpanStarted.HasValue || (ss != null && !source.Duration.HasValue)))
            {
                foreach (Span.Builder span in spans)
                {
                    span.Shared(true);
                }
            }

            // ms and mr are not supposed to be in the same span, but in case they are..
            if (ms != null && mr != null)
            {
                // special-case loopback: We need to make sure on loopback there are two span2s
                Span.Builder producer = forAnnotation(source, ms);
                producer.Kind(Span.SpanKind.PRODUCER);
                // fork a new span for the consumer side
                Span.Builder consumer = newSpanBuilder(source).Kind(Span.SpanKind.CONSUMER);

                consumer.Shared(true);
                if (wr != null)
                {
                    consumer.Timestamp(toMicroseconds(wr.Timestamp)).Duration(toMicroseconds(mr.Timestamp) - toMicroseconds(wr.Timestamp));
                }
                else
                {
                    consumer.Timestamp(toMicroseconds(mr.Timestamp));
                }

                producer.Timestamp(toMicroseconds(ms.Timestamp)).Duration(ws != null ? toMicroseconds(ws.Timestamp) - toMicroseconds(ms.Timestamp) : 0);
            }
            else if (ms != null)
            {
                maybeTimestampDuration(source, ms, ws);
            }
            else if (mr != null)
            {
                if (wr != null)
                {
                    maybeTimestampDuration(source, wr, mr);
                }
                else
                {
                    maybeTimestampDuration(source, mr, null);
                }
            }
            else
            {
                if (ws != null) forAnnotation(source, ws).AddAnnotation(toMicroseconds(ws.Timestamp), ws.Value);
                if (wr != null) forAnnotation(source, wr).AddAnnotation(toMicroseconds(wr.Timestamp), wr.Value);
            }
        }

        private void handleIncompleteRpc(zipkin4net.Tracers.Zipkin.Span source)
        {
            handleIncompleteRpc(first);
            foreach (Span.Builder span in spans)
            {
                handleIncompleteRpc(span);
            }
            if (source.SpanStarted.HasValue)
            {
                first.Timestamp(toMicroseconds(source.SpanStarted.Value)).Duration(toMicroseconds(source.Duration.Value));
            }
        }

        void handleIncompleteRpc(Span.Builder next)
        {
            if (Span.SpanKind.CLIENT.Equals(next.Kind()))
            {
                if (cs != null) next.Timestamp(toMicroseconds(cs.Timestamp));
                if (cr != null) next.AddAnnotation(toMicroseconds(cr.Timestamp), cr.Value);
            }
            else if (Span.SpanKind.SERVER.Equals(next.Kind()))
            {
                if (sr != null) next.Timestamp(toMicroseconds(sr.Timestamp));
                if (ss != null) next.AddAnnotation(toMicroseconds(ss.Timestamp), ss.Value);
            }
        }


        private Span.Builder newSpanBuilder(zipkin4net.Tracers.Zipkin.Span source)
        {
            Span.Builder result = newBuilder(Span.NewBuilder(), source);
            spans.Add(result);
            return result;

        }

        private void maybeTimestampDuration(zipkin4net.Tracers.Zipkin.Span source, ZipkinAnnotation begin, ZipkinAnnotation end)
        {
            Span.Builder span2 = first;
            if (source.SpanStarted.HasValue && source.Duration.HasValue)
            {
                span2.Timestamp(toMicroseconds(source.SpanStarted.Value)).Duration(toMicroseconds(source.Duration.Value));
            }
            else
            {
                span2.Timestamp(toMicroseconds(begin.Timestamp));
                if (end != null) span2.Duration(toMicroseconds(end.Timestamp) - toMicroseconds(begin.Timestamp));
            }

        }

        private void finish(List<Span> sink)
        {
            sink.Add(first.Build());
            foreach (Span.Builder span in spans)
            {
                sink.Add(span.Build());
            }
        }

        private void processBinaryAnnotations(zipkin4net.Tracers.Zipkin.Span source)
        {
            Endpoint ca = null, sa = null, ma = null;
            foreach (BinaryAnnotation b in source.BinaryAnnotations)
            {
                Endpoint bEndpoint = b.Host == null ? null : new Endpoint(b.Host.ServiceName, b.Host.IPEndPoint);
                // Peek to see if this is an address annotation. Strictly speaking, address annotations should
                // have a value of true (not "true" or "1"). However, there are versions of zipkin-ruby in the
                // wild that create "1" and misinterpreting confuses the question of what is the local
                // endpoint. Hence, we leniently parse.
                if ("ca".Equals(b.Key))
                {
                    ca = bEndpoint;
                    continue;
                }
                else if ("sa".Equals(b.Key))
                {
                    sa = bEndpoint;
                    continue;
                }
                else if ("ma".Equals(b.Key))
                {
                    ma = bEndpoint;
                    continue;
                }

                Span.Builder currentSpan = forEndpoint(source, bEndpoint);

                // don't add marker "lc" tags
                if ("lc".Equals(b.Key) && (b.Value == null || b.Value.Length == 0)) continue;
                currentSpan.PutTag(b.Key, Encoding.UTF8.GetString(b.Value));
            }

            bool noCoreAnnotations = cs == null && cr == null && ss == null && sr == null;
            // special-case when we are missing core annotations, but we have both address annotations
            if (noCoreAnnotations && (ca != null || sa != null))
            {
                if (ca != null && sa != null)
                {
                    forEndpoint(source, ca).RemoteEndpoint(sa);
                }
                else if (sa != null)
                {
                    // "sa" is a default for a remote address, don't make it a client span
                    forEndpoint(source, null).RemoteEndpoint(sa);
                }
                else
                { // ca != null: treat it like a server
                    forEndpoint(source, null).Kind(Span.SpanKind.SERVER).RemoteEndpoint(ca);
                }
                return;
            }

            ZipkinAnnotation server = sr != null ? sr : ss;
            Span.Builder serverSpan = server == null ? null : forAnnotation(source, server);
            if (ca != null && server != null && !ca.Equals(serverSpan.LocalEndpoint()))
            {
                // Finagle adds a "ca" annotation on server spans for the client-port on the socket, but with
                // the same service name as "sa". Removing the service name prevents creating loopback links.
                if (hasSameServiceName(ca, serverSpan.LocalEndpoint()))
                {
                    ca = new Endpoint(null, ca.HostPort);
                }
                serverSpan.RemoteEndpoint(ca);
            }
            if (sa != null)
            { // client span
                if (cs != null)
                {
                    forAnnotation(source, cs).RemoteEndpoint(sa);
                }
                else if (cr != null)
                {
                    forAnnotation(source, cr).RemoteEndpoint(sa);
                }
            }
            if (ma != null)
            { // messaging span
              // Intentionally process messaging endpoints separately in case someone accidentally shared
              // a messaging span. This will ensure both sides have the address of the broker.
                if (ms != null) forAnnotation(source, ms).RemoteEndpoint(ma);
                if (mr != null) forAnnotation(source, mr).RemoteEndpoint(ma);
            }
        }

        private Span.Builder forEndpoint(zipkin4net.Tracers.Zipkin.Span source, Endpoint e)
        {
            if (e == null) return first; // allocate missing endpoint data to first span
            if (closeEnoughEndpoint(first, e)) return first;
            foreach (Span.Builder next in spans)
            {
                if (closeEnoughEndpoint(next, e)) return next;
            }
            return newSpanBuilder(source).LocalEndpoint(e);

        }

        private bool closeEnoughEndpoint(Span.Builder builder, Endpoint e)
        {
            Endpoint localEndpoint = builder.LocalEndpoint();
            if (localEndpoint == null)
            {
                builder.LocalEndpoint(e);
                return true;
            }
            return hasSameServiceName(localEndpoint, e);
        }

        private bool hasSameServiceName(Endpoint left, Endpoint right)
        {
            return left.ServiceName.Equals(right.ServiceName);
        }

        void start(zipkin4net.Tracers.Zipkin.Span source)
        {
            first.Reset();
            spans.Clear();
            cs = sr = ss = cr = ms = mr = ws = wr = null;
            newBuilder(first, source);
        }

        Span.Builder forAnnotation(zipkin4net.Tracers.Zipkin.Span source, ZipkinAnnotation a)
        {
            Span.SpanKind? kind = null;
            if (a.Value.Length == 2)
            {
                if (a.Value.Equals("cs"))
                {
                    kind = Span.SpanKind.CLIENT;
                    cs = a;
                }
                else if (a.Value.Equals("sr"))
                {
                    kind = Span.SpanKind.SERVER;
                    sr = a;
                }
                else if (a.Value.Equals("ss"))
                {
                    kind = Span.SpanKind.SERVER;
                    ss = a;
                }
                else if (a.Value.Equals("cr"))
                {
                    kind = Span.SpanKind.CLIENT;
                    cr = a;
                }
                else if (a.Value.Equals("ms"))
                {
                    kind = Span.SpanKind.PRODUCER;
                    ms = a;
                }
                else if (a.Value.Equals("mr"))
                {
                    kind = Span.SpanKind.CONSUMER;
                    mr = a;
                }
                else if (a.Value.Equals("ws"))
                {
                    kind = Span.SpanKind.PRODUCER;
                    ws = a;
                }
                else if (a.Value.Equals("wr"))
                {
                    kind = Span.SpanKind.CONSUMER;
                    wr = a;
                }
                else
                {
                    first.AddAnnotation(toMicroseconds(a.Timestamp), a.Value);
                }
            }
            else
            {
                first.AddAnnotation(toMicroseconds(a.Timestamp), a.Value);
            }

            if (kind == null || !kind.HasValue) return first; // allocate missing endpoint data to first span
            if (closeSpanKind(first, kind.Value)) return first;
            foreach (Span.Builder next in spans)
            {
                if (closeSpanKind(next, kind.Value)) return next;
            }
            return newSpanBuilder(source).Kind(kind.Value);
        }

        static bool closeSpanKind(Span.Builder builder, Span.SpanKind kind)
        {
            if (!builder.HasKind())
            {
                builder.Kind(kind);
                return true;
            }
            return builder.Kind().Equals(kind);
        }

        private static Endpoint endpoint(zipkin4net.Tracers.Zipkin.Span source)
        {
            return new Endpoint(source.ServiceName, source.Endpoint);
        }


        private static bool endTimestampReflectsSpanDuration(ZipkinAnnotation end, zipkin4net.Tracers.Zipkin.Span source)
        {
            return end != null
                && source.SpanStarted.HasValue
                && source.Duration.HasValue
                && toMicroseconds(source.SpanStarted.Value) + toMicroseconds(source.Duration.Value) == toMicroseconds(end.Timestamp);
        }


        private static long toMicroseconds(DateTime dateTime)
        {
            return dateTime.ToUnixTimestamp();
        }

        private static long toMicroseconds(TimeSpan timeSpan)
        {
            return (long)(timeSpan.TotalMilliseconds * 1000);
        }


        private static Span.Builder newBuilder(Span.Builder builder, zipkin4net.Tracers.Zipkin.Span source)
        {
            var hexTraceIdHigh = TraceManager.Trace128Bits ? NumberUtils.EncodeLongToLowerHexString(source.SpanState.TraceIdHigh) : "";
            var traceId = hexTraceIdHigh + NumberUtils.EncodeLongToLowerHexString(source.SpanState.TraceId);
            builder.TraceId(hexTraceIdHigh + NumberUtils.EncodeLongToLowerHexString(source.SpanState.TraceId));
            if (!source.IsRoot)
            {
                builder.ParentId(NumberUtils.EncodeLongToLowerHexString(source.SpanState.ParentSpanId.Value));
            }
            builder.Id(NumberUtils.EncodeLongToLowerHexString(source.SpanState.SpanId));
            builder.Name(source.Name != null ? SerializerUtils.ToEscaped(source.Name) : SerializerUtils.DefaultRpcMethodName);
            builder.Debug(false);
            builder.LocalEndpoint(new Endpoint(source.ServiceName, source.Endpoint));
            return builder;
        }
    }
}