using Confluent.Kafka;

namespace Kafka.Entities.Common
{
    public class KafkaStringMessage : Message<string, string>
    {
        public string TopicName { get; set; }
    }
}
