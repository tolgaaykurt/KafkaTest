using Kafka.Entities.Common;
using Kafka.Producer.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kafka.Producer.Controllers
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
        private readonly KafkaProducerService _kafkaProducerService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, KafkaProducerService kafkaProducerService)
        {
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
        }

        [HttpGet(Name = "Get")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public IActionResult Post(KafkaStringMessage kafkaStringMessage)
        {
            IActionResult result = new OkResult();

            try
            {
                _kafkaProducerService.SendStringMessageAsync(kafkaStringMessage).Wait();
            }
            catch (Exception ex) 
            {
                result = new BadRequestObjectResult(ex);
            }

            return result;
        }

    }
}
