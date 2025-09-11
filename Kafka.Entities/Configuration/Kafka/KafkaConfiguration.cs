using System.ComponentModel.DataAnnotations;

namespace Kafka.Entities.Configuration.Kafka
{
    public sealed class KafkaConfiguration : IValidatableObject
    {
        public const string SETTING_NAME = "KafkaConfig";

        public required IList<ConfigItem> ProducerConfigItems { get; set; }

        public required IList<ConfigItem> ConsumerConfigItems { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "KafkaConfigItems değeri girilmelidir.")]
        public required IList<ConfigItem> KafkaConfigItems { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(KafkaConfigItems, new ValidationContext(KafkaConfigItems), validationResults, true);

            ValidateKafkaConfig(validationResults);
            ValidateProducerConfig(validationResults);
            ValidateConsumerConfig(validationResults);

            return validationResults;
        }

        /// <summary>
        /// app.settings dosyasındaki Kafka producer'ına ait config bilgilerini verir.
        /// </summary>
        /// <returns></returns>
        public IList<KeyValuePair<string, string>> GetProducerConfiguration()
        {
            IList<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>();

            if(ProducerConfigItems != null && ProducerConfigItems.Any())
            {
                GetKafkaConfiguration(ref keyValuePairs);
                GetConfigurationAsKeyValuePair(ProducerConfigItems, ref keyValuePairs);
            }

            return keyValuePairs;
        }

        /// <summary>
        /// app.settings dosyasındaki Kafka consumer'larına ait ortak config bilgilerini verir.
        /// </summary>
        /// <returns></returns>
        public IList<KeyValuePair<string, string>> GetConsumerConfiguration()
        {
            IList<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>();

            if (ProducerConfigItems != null && ProducerConfigItems.Any())
            {
                GetKafkaConfiguration(ref keyValuePairs);
                GetConfigurationAsKeyValuePair(ConsumerConfigItems, ref keyValuePairs);
            }

            return keyValuePairs;
        }

        private void GetKafkaConfiguration(ref IList<KeyValuePair<string, string>> resultList)
        {
            if(KafkaConfigItems != null && KafkaConfigItems.Any())
            {
                GetConfigurationAsKeyValuePair(KafkaConfigItems, ref resultList);
            }
        }

        private void GetConfigurationAsKeyValuePair(IList<ConfigItem> configItems, ref IList<KeyValuePair<string, string>> resultList)
        {
            if (configItems != null && configItems.Any())
            {
                foreach (ConfigItem item in configItems)
                {
                    resultList.Add(new KeyValuePair<string, string>(item.Name, item.Value));
                }
            }
        }

        private void ValidateKafkaConfig(IList<ValidationResult> validationResults)
        {
            if(KafkaConfigItems != null && KafkaConfigItems.Any() == true)
            {
                if(KafkaConfigItems.Any(t => t.Name == "bootstrap.servers") == false)
                {
                    validationResults.Add(new ValidationResult("bootstrap.servers değerini doldurunuz."));
                }

                ValidateConfigItems(KafkaConfigItems, validationResults);
            }
            else
            {
                validationResults.Add(new ValidationResult("KafkaConfig.KafkaConfigItems değerini doldurunuz."));
            }
        }

        private void ValidateConsumerConfig(IList<ValidationResult> validationResults)
        {
            if (ConsumerConfigItems != null && ConsumerConfigItems.Any() == true)
            {
                ValidateConfigItems(KafkaConfigItems, validationResults);
            }
        }

        private void ValidateProducerConfig(IList<ValidationResult> validationResults)
        {
            if (ProducerConfigItems != null && ProducerConfigItems.Any() == true)
            {
                ValidateConfigItems(ProducerConfigItems, validationResults);
            }
            else
            {
                validationResults.Add(new ValidationResult("KafkaConfig.ProducerConfigItems değerini doldurunuz."));
            }
        }

        private void ValidateConfigItems(IList<ConfigItem> configItems, IList<ValidationResult> validationResults)
        {
            if(configItems != null && configItems.Any())
            {
                foreach (ConfigItem item in KafkaConfigItems)
                {
                    Validator.TryValidateObject(item, new ValidationContext(item), validationResults, true);
                }
            }
        }
    }

    public class ConfigItem
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "ProducerConfig.Name değeri girilmelidir.")]
        public required string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "ProducerConfig.Name değeri girilmelidir.")]
        public required string Value { get; set; }
    }
}
