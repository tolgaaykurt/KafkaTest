using Confluent.Kafka;
using Kafka.Entities.Common;
using Kafka.Entities.Configuration.Kafka;
using Kafka.Producer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KafkaTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ProducerService _producerService;
        private readonly IOptions<KafkaConfiguration> _kafkaConfiguration;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ProducerService producerService, IOptions<KafkaConfiguration> kafkaConfiguration)
        {
            _logger = logger;
            _producerService = producerService;
            _kafkaConfiguration = kafkaConfiguration;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public int Get()
        {
            var sss = _kafkaConfiguration.Value.ProducerConfigItems;
            return 1;
        }

        [HttpPost(Name = "PostMessage")]
        public Task<bool> PostAsync(KafkaStringMessage kafkaStringMessage)
        {
            return _producerService.SendStringMessageAsync(kafkaStringMessage);
        }

        public void SendData()
        {
            /*#################################################################################################################################################
            * ## NOTLAR
            * max.in.flight.requests.per.connection mutlaka ayarlanmalı ve buna uygun buffer ve size ayarları yapılmalı.
              #################################################################################################################################################*/

            var producerConfig = new ProducerConfig();
            producerConfig.AllowAutoCreateTopics = true;
            producerConfig.BootstrapServers = "192.168.50.71:9092,192.168.50.124:9092,192.168.50.116:9092";
            //producerConfig.BootstrapServers = "192.168.50.71:9092";
            producerConfig.BrokerAddressTtl = 3600000;    // BootstrapServers parametresinde belirtilen broker adreslerinin ne süre cache'de tutulacağını belirtir.
            producerConfig.CancellationDelayMaxMs = 1000; // Bir işlem iptal edildiğinde (CancellationToken ile), Kafka arka planda hala devam etmekte olan bir çağrıyı temizlemeye çalışırken ne kadar süre bekleyeceğini belirler.
            producerConfig.ConnectionsMaxIdleMs = 0;
            producerConfig.ClientId = "Kafka_Producer";
            producerConfig.Acks = Acks.All;               // Üretilen mesaj tüm replikalara yazılana kadar producer thread'i block'lanır. Mesaj kaybı olmaması açısından en güvenli yöntem budur.
            producerConfig.MessageTimeoutMs = 5000;
            producerConfig.SocketConnectionSetupTimeoutMs = 3000;
            
            /*#################################################################################################################################################
             * ## MESAJ SIKIŞTIRMA AYARLARI YAPILIYOR
              #################################################################################################################################################*/
            producerConfig.CompressionLevel = 5;          // Sıkıştırma oranı ne kadar yüksek olursa cpu kullanımı o kadar artar. Kafka performansı yükselir.
            producerConfig.CompressionType = CompressionType.Gzip;

            /*#################################################################################################################################################
             * ## RETRY MEKANİZMASI AYARLANIYOR
              #################################################################################################################################################*/
            producerConfig.EnableIdempotence     = true;  // Retry edilen bir mesajın birden fazla kez kaydedilmesi engelleniyor. Aynı zamanda sırası da korunuyor.
            producerConfig.MessageSendMaxRetries = 3;     // Başarısız olan mesajın kaç kez retry edileceği ayarlanıyor.
            producerConfig.RetryBackoffMs        = 5000;  // Başarısız bir mesaj retry edilirken, retry'lar arasında ne kadar süre bekleneceğini ayarlar.

            using (var producer = new ProducerBuilder<string, string>(producerConfig).Build())
            {
                try
                {
                    var result = producer.ProduceAsync("customer-topic-2", new Message<string, string>
                    {
                        Key = "CustomerId",
                        Value = "CustomerInfo_Tolga_2"
                    });

                    result.Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            
        }
    }
}
