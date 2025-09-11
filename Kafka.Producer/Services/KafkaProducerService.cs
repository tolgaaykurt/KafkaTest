using Confluent.Kafka;
using Kafka.Entities.Common;
using Kafka.Entities.Configuration.Kafka;
using Microsoft.Extensions.Options;

namespace Kafka.Producer.Services
{
    public class KafkaProducerService : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;
        private IList<ConfigItem> _previousConfig;
        private List<IProducer<string, string>> _producers;
        private bool _disposed;

        public KafkaProducerService(IOptionsMonitor<KafkaConfiguration> kafkaConfiguration)
        {
            _lock = new ReaderWriterLockSlim();
            _disposed = false;
            _producers = new List<IProducer<string, string>>();
            _previousConfig = kafkaConfiguration.CurrentValue.ProducerConfigItems;

            CreateProducerAsync(kafkaConfiguration.CurrentValue).Wait();
            kafkaConfiguration.OnChange(options => this.ProducerConfigurationsOnChange(kafkaConfiguration.CurrentValue));

        }

        public async Task<bool> SendStringMessageAsync(KafkaStringMessage kafkaStringMessage)
        {
            bool taskResult = false;

            try
            {
                var messageResult = await GetProducer().ProduceAsync(kafkaStringMessage.TopicName, kafkaStringMessage);

                if (messageResult.Status == PersistenceStatus.Persisted) taskResult = true; else taskResult = false;

                //todo: Eğer Persisted harici bir status geldiyse bunu mutlaka log dosyasına yaz.
            }
            catch (Exception ex)
            {
                //todo: Log ekle.
                taskResult = false;
                Console.WriteLine($"KAFKA HATA: {ex.Message}");
            }

            return taskResult;
        }

        /// <summary>
        /// Aktif producer listesinin sonuna yeni bir producer ekler.
        /// </summary>
        /// <param name="producerSettings"></param>
        private async Task CreateProducerAsync(KafkaConfiguration producerConfig)
        {
            _lock.EnterWriteLock();

            try
            {
                _producers.Add(new ProducerBuilder<string, string>(producerConfig.GetProducerConfiguration()).Build());
            }
            catch (Exception e)
            {
                //todo: Log ekle.
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private IProducer<string, string> GetProducer()
        {
            IProducer<string, string>? producer = null;
            _lock.EnterReadLock();

            try
            {
                producer = _producers.Last();
            }
            catch (Exception e)
            {
                //todo: Log ekle.
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return producer!;
        }


        /// <summary>
        /// Producer listesindeki ilk producer'ı flush ve dispose eder.
        /// </summary>
        private async Task DisposeOldProducerAsync()
        {
            var firstProducer = _producers.First<IProducer<string, string>>();
            firstProducer.Flush();
            firstProducer.Dispose();

            _lock.EnterWriteLock();

            try
            {
                _producers.Remove(firstProducer);
            }
            catch (Exception e)
            {
                //todo: Log ekle.
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private async Task DisposeAllProducersAsync()
        {
            if (_producers != null && _producers.Any())
            {
                _lock.EnterWriteLock();

                try
                {
                    foreach (var producer in _producers)
                    {
                        try
                        {
                            producer.Flush();
                            producer.Dispose();
                        }
                        catch (Exception ex)
                        {
                            //todo: Log ekle.
                            string error = $"{producer.Name} isimli producer flush veya dispose edilemedi. Mesaj kaybı olmuş olabilir.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    //todo: Log ekle.
                }
                finally
                {
                    _producers.Clear();
                    _lock.ExitWriteLock();
                }
            }
        }

        private async void ProducerConfigurationsOnChange(KafkaConfiguration kafkaConfiguration)
        {
            if(_previousConfig.Equals(kafkaConfiguration.ProducerConfigItems) == false)
            {
                _previousConfig = kafkaConfiguration.ProducerConfigItems;
                await CreateProducerAsync(kafkaConfiguration);
                DisposeOldProducerAsync();
            }
        }

        public async void Dispose()
        {
            if(_disposed == false)
            {
                if(_producers != null && _producers.Any())
                {
                    _disposed = true;
                    await DisposeAllProducersAsync();                    
                }
            }
        }
    }
}
