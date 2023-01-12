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

﻿using System.Collections.Generic;
using System.IO;
using System.Net;
using System;
using easeagent.Tracers.Zipkin.V2;

namespace easeagent.Tracers.Zipkin
{
    public class JSONSpanSerializerV2 : zipkin4net.Tracers.Zipkin.ISpanSerializer
    {
        internal const char openingBrace = '{';
        internal const char closingBrace = '}';
        internal const char comma = ',';
        private const string annotations = "annotations";
        private const string localEndpoint = "localEndpoint";
        private const string remoteEndpoint = "remoteEndpoint";
        private const string timestamp = "timestamp";
        private const string duration = "duration";
        private const string value = "value";
        private const string id = "id";
        private const string traceId = "traceId";
        private const string parentId = "parentId";
        private const string debug = "debug";
        private const string name = "name";
        private const string ipv4 = "ipv4";
        private const string port = "port";
        private const string shared = "shared";
        private const string kind = "kind";
        private const string tags = "tags";
        private const string serviceName = "serviceName";
        private const string service = "service";
        private const string tracingType = "type";

        public string ServiceName { get; }
        public string TracingType { get; }


        public JSONSpanSerializerV2(string ServiceName, string TracingType)
        {
            this.ServiceName = ServiceName;
            this.TracingType = TracingType;
        }

        public void SerializeTo(Stream stream, zipkin4net.Tracers.Zipkin.Span span)
        {
            using (var writer = new StreamWriter(stream))
            {
                List<Span> spans = V1SpanConverter.Create().Convert(span);
                writer.WriteList(SerializeSpan, spans);
            }
        }

        private void SerializeSpan(StreamWriter writer, Span span)
        {
            var serviceName = getServiceNameOrDefault(span);
            writer.Write(openingBrace);

            //id
            writer.WriteField(id, span.Id);

            //traceId
            writer.Write(comma);
            writer.WriteField(traceId, span.TraceId);

            //timestamp
            writer.Write(comma);
            writer.WriteField(timestamp, span.Timestamp);


            //parentId
            if (span.ParentId != null)
            {
                writer.Write(comma);
                writer.WriteField(parentId, span.ParentId);
            }


            //name
            writer.Write(comma);
            writer.WriteField(name, span.Name);

            //duration
            writer.Write(comma);
            writer.WriteField(duration, span.Duration);

            //local endpoint
            if (span.LocalEndpoint != null)
            {
                SerializeLocalEndPoint(writer, span.LocalEndpoint.HostPort, this.ServiceName);
            }

            //debug
            if (span.Debug.HasValue)
            {
                writer.Write(comma);
                writer.WriteField(debug, span.Debug.Value);

            }

            //shared
            if (span.Shared.HasValue)
            {
                writer.Write(comma);
                writer.WriteField(shared, span.Shared.Value);
            }

            //kind
            writer.Write(comma);
            writer.WriteField(kind, span.Kind.ToString());

            var remoteEndpoint = getRemoteEndpoint(span);
            if (remoteEndpoint != null)
            {
                SerializeRemoteEndPoint(writer, remoteEndpoint.HostPort, remoteEndpoint.ServiceName);
            }

            if (span.Annotations != null && span.Annotations.Count != 0)
            {
                writer.Write(comma);
                writer.WriteList(
                    SerializeAnnotation,
                    annotations,
                    span.Annotations
                );
            }
            if (span.Tags != null && span.Tags.Count != 0)
            {
                writer.Write(comma);
                writer.WriteMap(SerializeTags, tags, span.Tags);
            }

            writer.Write(comma);
            writer.WriteField(service, this.ServiceName);
            writer.Write(comma);
            writer.WriteField(tracingType, this.TracingType);
            writer.Write(closingBrace);
        }


        private static void SerializeAnnotation(StreamWriter writer, Annotation annotation)
        {
            writer.Write(openingBrace);
            writer.WriteField(timestamp, annotation.Timestamp);
            writer.Write(comma);
            writer.WriteField(value, zipkin4net.Tracers.Zipkin.SerializerUtils.ToEscaped(annotation.Value));
            writer.Write(closingBrace);
        }

        private static void SerializeLocalEndPoint(StreamWriter writer, IPEndPoint endPoint, string serviceName)
        {
            writer.Write(comma);
            writer.WriteAnchor(localEndpoint);
            SerializeEndPoint(writer, endPoint, serviceName);
        }
        private static void SerializeRemoteEndPoint(StreamWriter writer, IPEndPoint endPoint, string serviceName)
        {
            writer.Write(comma);
            writer.WriteAnchor(remoteEndpoint);
            SerializeEndPoint(writer, endPoint, serviceName);
        }

        private static void SerializeEndPoint(StreamWriter writer, IPEndPoint endPoint, string serviceName)
        {
            writer.Write(openingBrace);
            if (endPoint != null)
            {
                writer.WriteField(ipv4, zipkin4net.Tracers.Zipkin.SerializerUtils.IpToString(endPoint.Address));
                writer.Write(comma);
                writer.WriteField(port, (short)endPoint.Port);
                if (serviceName != null)
                {
                    writer.Write(comma);
                }
            }
            if (serviceName != null)
            {
                writer.WriteField(JSONSpanSerializerV2.serviceName, zipkin4net.Tracers.Zipkin.SerializerUtils.ToEscaped(serviceName));
            }
            writer.Write(closingBrace);
        }

        private static void SerializeTags(StreamWriter writer, KeyValuePair<string, string> tag)
        {
            writer.WriteField(tag.Key, tag.Value);
        }

        private static String getServiceNameOrDefault(Span span)
        {
            return span.LocalServiceName != null ? span.LocalServiceName : zipkin4net.Tracers.Zipkin.SerializerUtils.DefaultServiceName;
        }

        private Endpoint getRemoteEndpoint(Span span)
        {
            var tags = span.Tags;
            if (tags == null || !tags.ContainsKey(easeagent.Middleware.TypeExtensions.TAG))
            {
                return span.RemoteEndpoint;
            }
            var middlewareType = tags[easeagent.Middleware.TypeExtensions.TAG];
            var endpoint = span.RemoteEndpoint;
            if (endpoint == null)
            {
                return new Endpoint(middlewareType, null);
            }
            if (string.IsNullOrEmpty(endpoint.ServiceName))
            {
                return new Endpoint(middlewareType, endpoint.HostPort);
            }
            return endpoint;
        }
    }


    delegate void SerializeMethod<T>(StreamWriter writer, T element);

    internal static class WriterExtensions
    {
        private const char quotes = '"';
        private const char colon = ':';
        internal const char openingBracket = '[';
        internal const char closingBracket = ']';
        internal static void WriteList<U>
        (
            this StreamWriter writer,
            SerializeMethod<U> serializer,
            string fieldName,
            ICollection<U> elements
        )
        {
            WriteAnchor(writer, fieldName);
            WriteList(writer, serializer, elements);
        }

        internal static void WriteList<U>
        (
            this StreamWriter writer,
            SerializeMethod<U> serializer,
            ICollection<U> elements
        )
        {
            writer.Write(openingBracket);
            WriteU(writer, serializer, elements);
            writer.Write(closingBracket);
        }

        private static void WriteU<U>
        (
            this StreamWriter writer,
            SerializeMethod<U> serializer,
            ICollection<U> elements
        )
        {
            bool firstElement = true;
            foreach (var element in elements)
            {
                if (firstElement == true)
                {
                    firstElement = false;
                }
                else
                {
                    writer.Write(JSONSpanSerializerV2.comma);
                }
                serializer(writer, element);
            }
        }

        internal static void WriteMap<U>
        (
            this StreamWriter writer,
            SerializeMethod<U> serializer,
            string fieldName,
            ICollection<U> elements
        )
        {
            WriteAnchor(writer, fieldName);
            WriteMap(writer, serializer, elements);
        }

        internal static void WriteMap<U>
        (
            this StreamWriter writer,
            SerializeMethod<U> serializer,
            ICollection<U> elements
        )
        {
            writer.Write(JSONSpanSerializerV2.openingBrace);
            WriteU(writer, serializer, elements);
            writer.Write(JSONSpanSerializerV2.closingBrace);
        }

        internal static void WriteField
        (
            this StreamWriter writer,
            string fieldName,
            bool fieldValue
        )
        {
            WriteAnchor(writer, fieldName);
            writer.Write(fieldValue ? "true" : "false");
        }

        internal static void WriteField
        (
            this StreamWriter writer,
            string fieldName,
            string fieldValue
        )
        {
            WriteAnchor(writer, fieldName);
            writer.Write(quotes);
            writer.Write(fieldValue);
            writer.Write(quotes);
        }
        internal static void WriteField
        (
            this StreamWriter writer,
            string fieldName,
            long fieldValue
        )
        {
            WriteAnchor(writer, fieldName);
            writer.Write(fieldValue);
        }

        internal static void WriteAnchor(this StreamWriter writer, string anchor)
        {
            writer.Write(quotes);
            writer.Write(anchor);
            writer.Write(quotes);
            writer.Write(colon);
        }
    }
}