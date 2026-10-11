using System.Text.Json;
using Confluent.Kafka;

namespace HospitalDashboard.Api.Messaging;

public record AdmissionCreated(
    Guid EventId, string Type, Guid AdmissionId, Guid PatientId,
    Guid BedId, Guid StaffId, DateTime AdmittedAt, DateTime OccurredAt);

public class EventPublisher : IDisposable
{
    private readonly IProducer<string, string> _producer;

    public EventPublisher(IConfiguration config)
    {
        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092",
            Acks = Acks.All,
            EnableIdempotence = true
        }).Build();
    }

    public Task PublishAsync<T>(string topic, string key, T evt) =>
        _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = key,
            Value = JsonSerializer.Serialize(evt)
        });

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}