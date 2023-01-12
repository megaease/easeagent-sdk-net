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

namespace easeagent;
public class Constant
{
    // For details, please see https://github.com/megaease/easeagent-sdk-net/blob/main/doc/middleware-span.md

    public static readonly string ERROR = "ERROR";
    public static readonly string UNKNOWN = "unknown";
    public static readonly string HTTP_TAG_ATTRIBUTE_ROUTE = "http.route";
    public static readonly string HTTP_TAG_METHOD = "http.method";
    public static readonly string HTTP_TAG_PATH = "http.path";
    public static readonly string HTTP_TAG_STATUS_CODE = "http.status_code";
    public static readonly string HTTP_TAG_SCRIPT_FILENAME = "http.script.filename";
    public static readonly string HTTP_TAG_CLIENT_ADDRESS = "Client Address";

    public static readonly string MYSQL_TAG_SQL = "sql";
    public static readonly string MYSQL_TAG_URL = "url";

    public static readonly string REDIS_TAG_METHOD = "redis.method";

    public static readonly string ELASTICSEARCH_TAG_INDEX = "es.index";
    public static readonly string ELASTICSEARCH_TAG_OPERATION = "es.operation";
    public static readonly string ELASTICSEARCH_TAG_BODY = "es.body";

    public static readonly string KAFKA_TAG_TOPIC = "kafka.topic";
    public static readonly string KAFKA_TAG_KEY = "kafka.key";
    public static readonly string KAFKA_TAG_BROKER = "kafka.broker";

    public static readonly string RABBIT_TAG_EXCHANGE = "rabbit.exchange";
    public static readonly string RABBIT_TAG_ROUTING_KEY = "rabbit.routing_key";
    public static readonly string RABBIT_TAG_QUEUE = "rabbit.queue";
    public static readonly string RABBIT_TAG_BROKER = "rabbit.broker";

    public static readonly string MONGODB_TAG_COMMAND = "mongodb.command";
    public static readonly string MONGODB_TAG_COLLECTION = "mongodb.collection";
    public static readonly string MONGODB_TAG_CLUSTER_ID = "mongodb.cluster_id";
}