using Kafka.Entities.Configuration.Kafka;
using Kafka.Producer.Services;

namespace Kafka.Producer.Extensions
{
    public static class KafkaProducerExtensions
    {
        public static void AddKafkaProducer(this WebApplicationBuilder builder)
        {
            builder.Services.AddOptions<KafkaConfiguration>()
            .Bind(builder.Configuration.GetSection(KafkaConfiguration.SETTING_NAME))
            .ValidateDataAnnotations()
            .ValidateOnStart();

            builder.Services.AddSingleton<KafkaProducerService>();
        }
    }
}
