using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace easeagent;
public class Constant
{
    // For details, please see https://github.com/megaease/easeagent-sdk-php/blob/main/doc/middleware-span.md

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