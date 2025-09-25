using Confluent.Kafka;

namespace KafkaEventsApi
{
	// --- Hosted Service для потребления Kafka сообщений ---
	public class KafkaConsumerService : BackgroundService
	{
		private readonly string[] topics = new[] { "movie-events", "user-events", "payment-events" };

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var kafkaBrokers = Environment.GetEnvironmentVariable("KAFKA_BROKERS") ?? "localhost:9092";

			Task.Run(() =>
			{
				var config = new ConsumerConfig
				{
					BootstrapServers = kafkaBrokers,
					GroupId = "event-logger-group",
					AutoOffsetReset = AutoOffsetReset.Earliest
				};

				using var consumer = new ConsumerBuilder<string, string>(config).Build();
				consumer.Subscribe(topics);

				while (!stoppingToken.IsCancellationRequested)
				{
					try
					{
						var cr = consumer.Consume(stoppingToken);
						Console.WriteLine($"[Kafka Log] Topic: {cr.Topic}, Key: {cr.Message.Key}, Value: {cr.Message.Value}");
					}
					catch (ConsumeException e)
					{
						Console.WriteLine($"Error consuming Kafka message: {e.Error.Reason}");
					}
				}
			});

			return Task.CompletedTask;
		}
	}
}