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