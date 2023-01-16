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
using NUnit.Framework;
using easeagent.Middleware;

namespace easeagent.UTest.Middleware
{
    [TestFixture]
    internal class T_Type
    {
        [Test]
        public void Value()
        {
            Assert.AreEqual("redis", Type.Redis.Value());
            Assert.AreEqual("elasticsearch", Type.ElasticSearch.Value());
            Assert.AreEqual("kafka", Type.Kafka.Value());
            Assert.AreEqual("rabbitmq", Type.RabbitMQ.Value());
            Assert.AreEqual("mongodb", Type.MongoDB.Value());
            Assert.AreEqual("motan", Type.Motan.Value());
        }
    }
}