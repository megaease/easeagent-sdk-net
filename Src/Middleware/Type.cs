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

        public static string Value(this Type type)
        {
            string result = "unknow";
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
