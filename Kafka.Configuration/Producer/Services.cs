using Kafka.Entities.Configuration.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Configuration.Producer
{
    public static class Services
    {
        public static void AddKafkaProducer(this WebApplicationBuilder builder)
        {
            builder.Services.AddOptions<KafkaConfiguration>()
            .Bind(builder.Configuration.GetSection(KafkaConfiguration.SETTING_NAME))
            .ValidateDataAnnotations()
            .ValidateOnStart();


        }
    }
}
