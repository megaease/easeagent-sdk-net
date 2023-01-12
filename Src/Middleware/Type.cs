namespace easeagent.Middleware
{
    public enum Type
    {
        MySql,
        Redis,
        ElasticSearch,
        Kafka,
        RabbitMQ,
        MongoDB,
        Motan
    }

    public static class TypeExtensions
    {
        public const string TAG = "component.type";
        public static string value(this Type type)
        {
            string result = "";
            switch (type)
            {
                case Type.MySql:
                    result = "database";
                    break;
                case Type.Redis:
                    result = "redis";
                    break;
                case Type.ElasticSearch:
                    result = "elasticsearch";
                    break;
                case Type.Kafka:
                    result = "kafka";
                    break;
                case Type.RabbitMQ:
                    result = "rabbitmq";
                    break;
                case Type.MongoDB:
                    result = "mongodb";
                    break;
                case Type.Motan:
                    result = "motan";
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}