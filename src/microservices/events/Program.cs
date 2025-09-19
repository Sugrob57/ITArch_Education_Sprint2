using Confluent.Kafka;
using KafkaEventsApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

// --- Конфигурация Kafka ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8082";
var kafkaBrokers = Environment.GetEnvironmentVariable("KAFKA_BROKERS") ?? "localhost:9092";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
	options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Kafka Producer
builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
	var config = new ProducerConfig
	{
		BootstrapServers = kafkaBrokers
	};
	return new ProducerBuilder<string, string>(config).Build();
});

// Kafka Consumer (для логирования)
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

// --- API для проверки состояния ---
app.MapGet("/api/events/health", () => Results.Json(new { status = true }));

// --- API для публикации MovieEvent ---
app.MapPost("/api/events/movie", async (MovieEvent movieEvent, IProducer<string, string> producer) =>
{
	var evt = new Event($"movie-{movieEvent.MovieId}-{movieEvent.Action}", "movie", DateTime.UtcNow, movieEvent);
	var message = new Message<string, string> { Key = evt.Id, Value = JsonSerializer.Serialize(evt) };

	var result = await producer.ProduceAsync("movie-events", message);

	return Results.Created("", new EventResponse("success", result.Partition.Value, result.Offset.Value, evt));
});

// --- API для публикации UserEvent ---
app.MapPost("/api/events/user", async (UserEvent userEvent, IProducer<string, string> producer) =>
{
	var evt = new Event($"user-{userEvent.UserId}-{userEvent.Action}", "user", DateTime.UtcNow, userEvent);
	var message = new Message<string, string> { Key = evt.Id, Value = JsonSerializer.Serialize(evt) };

	var result = await producer.ProduceAsync("user-events", message);

	return Results.Created("", new EventResponse("success", result.Partition.Value, result.Offset.Value, evt));
});

// --- API для публикации PaymentEvent ---
app.MapPost("/api/events/payment", async (PaymentEvent paymentEvent, IProducer<string, string> producer) =>
{
	var evt = new Event($"payment-{paymentEvent.PaymentId}", "payment", DateTime.UtcNow, paymentEvent);
	var message = new Message<string, string> { Key = evt.Id, Value = JsonSerializer.Serialize(evt) };

	var result = await producer.ProduceAsync("payment-events", message);

	return Results.Created("", new EventResponse("success", result.Partition.Value, result.Offset.Value, evt));
});

app.Run($"http://0.0.0.0:{port}");