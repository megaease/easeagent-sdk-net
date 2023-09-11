/**
 * Copyright 2023 MegaEase
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
            Convert(source, results);
            return results;
        }

        private void Convert(zipkin4net.Tracers.Zipkin.Span source, List<Span> sink)
        {
            Start(source);
            // add annotations unless they are "core"
            ProcessAnnotations(source);
            // convert binary annotations to tags and addresses
            ProcessBinaryAnnotations(source);
            Finish(sink);
        }


        private void ProcessAnnotations(zipkin4net.Tracers.Zipkin.Span source)
        {
            Endpoint sourceEndpoint = Endpoint(source);
            foreach (ZipkinAnnotation a in source.Annotations)
            {
                ForAnnotation(source, a);
            }

            // When bridging between event and span model, you can end up missing a start annotation
            if (cs == null && EndTimestampReflectsSpanDuration(cr, source))
            {
                cs = new ZipkinAnnotation(source.SpanStarted.Value, "cs");
            }
            if (sr == null && EndTimestampReflectsSpanDuration(ss, source))
            {
                sr = new ZipkinAnnotation(source.SpanStarted.Value, "sr");
            }

            if (cs != null && sr != null)
            {
                // in a shared span, the client side owns span duration by annotations or explicit timestamp
                MaybeTimestampDuration(source, cs, cr);

                // special-case loopback: We need to make sure on loopback there are two span2s
                Span.Builder client = ForAnnotation(source, cs);
                client.Kind(Span.SpanKind.CLIENT);
                // fork a new span for the server side
                Span.Builder server = NewSpanBuilder(source).Kind(Span.SpanKind.SERVER);

                // the server side is smaller than that, we have to read annotations to find out
                server.Shared(true).Timestamp(ToMicroseconds(sr.Timestamp));
                if (ss != null) server.Duration(ToMicroseconds(ss.Timestamp) - ToMicroseconds(sr.Timestamp));
                if (cr == null && !source.Duration.HasValue) client.Duration(0); // one-way has no duration
            }
            else if (cs != null && cr != null)
            {
                MaybeTimestampDuration(source, cs, cr);
            }
            else if (sr != null && ss != null)
            {
                MaybeTimestampDuration(source, sr, ss);
            }
            else
            { // otherwise, the span is incomplete. revert special-casing
                HandleIncompleteRpc(source);
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
                Span.Builder producer = ForAnnotation(source, ms);
                producer.Kind(Span.SpanKind.PRODUCER);
                // fork a new span for the consumer side
                Span.Builder consumer = NewSpanBuilder(source).Kind(Span.SpanKind.CONSUMER);

                consumer.Shared(true);
                if (wr != null)
                {
                    consumer.Timestamp(ToMicroseconds(wr.Timestamp)).Duration(ToMicroseconds(mr.Timestamp) - ToMicroseconds(wr.Timestamp));
                }
                else
                {
                    consumer.Timestamp(ToMicroseconds(mr.Timestamp));
                }

                producer.Timestamp(ToMicroseconds(ms.Timestamp)).Duration(ws != null ? ToMicroseconds(ws.Timestamp) - ToMicroseconds(ms.Timestamp) : 0);
            }
            else if (ms != null)
            {
                MaybeTimestampDuration(source, ms, ws);
            }
            else if (mr != null)
            {
                if (wr != null)
                {
                    MaybeTimestampDuration(source, wr, mr);
                }
                else
                {
                    MaybeTimestampDuration(source, mr, null);
                }
            }
            else
            {
                if (ws != null) ForAnnotation(source, ws).AddAnnotation(ToMicroseconds(ws.Timestamp), ws.Value);
                if (wr != null) ForAnnotation(source, wr).AddAnnotation(ToMicroseconds(wr.Timestamp), wr.Value);
            }
        }

        private void HandleIncompleteRpc(zipkin4net.Tracers.Zipkin.Span source)
        {
            HandleIncompleteRpc(first);
            foreach (Span.Builder span in spans)
            {
                HandleIncompleteRpc(span);
            }
            if (source.SpanStarted.HasValue)
            {
                first.Timestamp(ToMicroseconds(source.SpanStarted.Value)).Duration(ToMicroseconds(source.Duration.Value));
            }
        }

        private void HandleIncompleteRpc(Span.Builder next)
        {
            if (Span.SpanKind.CLIENT.Equals(next.Kind()))
            {
                if (cs != null) next.Timestamp(ToMicroseconds(cs.Timestamp));
                if (cr != null) next.AddAnnotation(ToMicroseconds(cr.Timestamp), cr.Value);
            }
            else if (Span.SpanKind.SERVER.Equals(next.Kind()))
            {
                if (sr != null) next.Timestamp(ToMicroseconds(sr.Timestamp));
                if (ss != null) next.AddAnnotation(ToMicroseconds(ss.Timestamp), ss.Value);
            }
        }


        private Span.Builder NewSpanBuilder(zipkin4net.Tracers.Zipkin.Span source)
        {
            Span.Builder result = NewBuilder(Span.NewBuilder(), source);
            spans.Add(result);
            return result;

        }

        private void MaybeTimestampDuration(zipkin4net.Tracers.Zipkin.Span source, ZipkinAnnotation begin, ZipkinAnnotation end)
        {
            Span.Builder span2 = first;
            if (source.SpanStarted.HasValue && source.Duration.HasValue)
            {
                span2.Timestamp(ToMicroseconds(source.SpanStarted.Value)).Duration(ToMicroseconds(source.Duration.Value));
            }
            else
            {
                span2.Timestamp(ToMicroseconds(begin.Timestamp));
                if (end != null) span2.Duration(ToMicroseconds(end.Timestamp) - ToMicroseconds(begin.Timestamp));
            }

        }

        private void Finish(List<Span> sink)
        {
            sink.Add(first.Build());
            foreach (Span.Builder span in spans)
            {
                sink.Add(span.Build());
            }
        }

        private void ProcessBinaryAnnotations(zipkin4net.Tracers.Zipkin.Span source)
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

                Span.Builder currentSpan = ForEndpoint(source, bEndpoint);

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
                    ForEndpoint(source, ca).RemoteEndpoint(sa);
                }
                else if (sa != null)
                {
                    // "sa" is a default for a remote address, don't make it a client span
                    ForEndpoint(source, null).RemoteEndpoint(sa);
                }
                else
                { // ca != null: treat it like a server
                    ForEndpoint(source, null).Kind(Span.SpanKind.SERVER).RemoteEndpoint(ca);
                }
                return;
            }

            ZipkinAnnotation server = sr != null ? sr : ss;
            Span.Builder serverSpan = server == null ? null : ForAnnotation(source, server);
            if (ca != null && server != null && !ca.Equals(serverSpan.LocalEndpoint()))
            {
                // Finagle adds a "ca" annotation on server spans for the client-port on the socket, but with
                // the same service name as "sa". Removing the service name prevents creating loopback links.
                if (HasSameServiceName(ca, serverSpan.LocalEndpoint()))
                {
                    ca = new Endpoint(null, ca.HostPort);
                }
                serverSpan.RemoteEndpoint(ca);
            }
            if (sa != null)
            { // client span
                if (cs != null)
                {
                    ForAnnotation(source, cs).RemoteEndpoint(sa);
                }
                else if (cr != null)
                {
                    ForAnnotation(source, cr).RemoteEndpoint(sa);
                }
            }
            if (ma != null)
            { // messaging span
              // Intentionally process messaging endpoints separately in case someone accidentally shared
              // a messaging span. This will ensure both sides have the address of the broker.
                if (ms != null) ForAnnotation(source, ms).RemoteEndpoint(ma);
                if (mr != null) ForAnnotation(source, mr).RemoteEndpoint(ma);
            }
        }

        private Span.Builder ForEndpoint(zipkin4net.Tracers.Zipkin.Span source, Endpoint e)
        {
            if (e == null) return first; // allocate missing endpoint data to first span
            if (CloseEnoughEndpoint(first, e)) return first;
            foreach (Span.Builder next in spans)
            {
                if (CloseEnoughEndpoint(next, e)) return next;
            }
            return NewSpanBuilder(source).LocalEndpoint(e);

        }

        private bool CloseEnoughEndpoint(Span.Builder builder, Endpoint e)
        {
            Endpoint localEndpoint = builder.LocalEndpoint();
            if (localEndpoint == null)
            {
                builder.LocalEndpoint(e);
                return true;
            }
            return HasSameServiceName(localEndpoint, e);
        }

        private bool HasSameServiceName(Endpoint left, Endpoint right)
        {
            return left.ServiceName.Equals(right.ServiceName);
        }

        void Start(zipkin4net.Tracers.Zipkin.Span source)
        {
            first.Reset();
            spans.Clear();
            cs = sr = ss = cr = ms = mr = ws = wr = null;
            NewBuilder(first, source);
        }

        Span.Builder ForAnnotation(zipkin4net.Tracers.Zipkin.Span source, ZipkinAnnotation a)
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
                    first.AddAnnotation(ToMicroseconds(a.Timestamp), a.Value);
                }
            }
            else
            {
                first.AddAnnotation(ToMicroseconds(a.Timestamp), a.Value);
            }

            if (kind == null || !kind.HasValue) return first; // allocate missing endpoint data to first span
            if (CloseSpanKind(first, kind.Value)) return first;
            foreach (Span.Builder next in spans)
            {
                if (CloseSpanKind(next, kind.Value)) return next;
            }
            return NewSpanBuilder(source).Kind(kind.Value);
        }

        static bool CloseSpanKind(Span.Builder builder, Span.SpanKind kind)
        {
            if (!builder.HasKind())
            {
                builder.Kind(kind);
                return true;
            }
            return builder.Kind().Equals(kind);
        }

        private static Endpoint Endpoint(zipkin4net.Tracers.Zipkin.Span source)
        {
            return new Endpoint(source.ServiceName, source.Endpoint);
        }


        private static bool EndTimestampReflectsSpanDuration(ZipkinAnnotation end, zipkin4net.Tracers.Zipkin.Span source)
        {
            return end != null
                && source.SpanStarted.HasValue
                && source.Duration.HasValue
                && ToMicroseconds(source.SpanStarted.Value) + ToMicroseconds(source.Duration.Value) == ToMicroseconds(end.Timestamp);
        }


        private static long ToMicroseconds(DateTime dateTime)
        {
            return dateTime.ToUnixTimestamp();
        }

        private static long ToMicroseconds(TimeSpan timeSpan)
        {
            return (long)(timeSpan.TotalMilliseconds * 1000);
        }


        private static Span.Builder NewBuilder(Span.Builder builder, zipkin4net.Tracers.Zipkin.Span source)
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