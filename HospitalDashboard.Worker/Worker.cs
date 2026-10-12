using System.Text.Json;
using Confluent.Kafka;
using HospitalDashboard.Api;
using HospitalDashboard.Api.Models;
using Microsoft.EntityFrameworkCore;

public record AdmissionCreated(Guid EventId, string Type, Guid AdmissionId, Guid PatientId,
    Guid BedId, Guid StaffId, DateTime AdmittedAt, DateTime OccurredAt);

public record PatientDischarged(Guid EventId, string Type, Guid AdmissionId, Guid BedId,
    Guid StaffId, DateTime DischargedAt, DateTime OccurredAt);

public record BedStatusChanged(Guid EventId, string Type, Guid BedId, string Status,
    Guid AdmissionId, DateTime OccurredAt);

public class Worker(IServiceScopeFactory scopes, IConfiguration config, ILogger<Worker> log) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken ct) => Task.Run(() => Run(ct), ct);

    private async Task Run(CancellationToken ct)
    {
        var servers = config["Kafka:BootstrapServers"] ?? "localhost:9092";

        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = servers,
            GroupId = "bed-event-processor",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        }).Build();

        using var producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = servers,
            Acks = Acks.All,
            EnableIdempotence = true
        }).Build();

        consumer.Subscribe("bed-events");
        log.LogInformation("Worker listening on bed-events...");

        while (!ct.IsCancellationRequested)
        {
            ConsumeResult<string, string>? msg;
            try { msg = consumer.Consume(ct); }
            catch (OperationCanceledException) { break; }
            if (msg is null) continue;

            try
            {
                await Dispatch(msg.Message.Value, producer, ct);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Event failed, sending to bed-events.dlq");
                await producer.ProduceAsync("bed-events.dlq",
                    new Message<string, string> { Key = msg.Message.Key, Value = msg.Message.Value }, ct);
            }

            consumer.Commit(msg);
        }

        consumer.Close();
    }

    private async Task Dispatch(string json, IProducer<string, string> producer, CancellationToken ct)
    {
        string type;
        using (var doc = JsonDocument.Parse(json))
            type = doc.RootElement.GetProperty("Type").GetString() ?? "";

        switch (type)
        {
            case "AdmissionCreated": await HandleAdmission(json, producer, ct); break;
            case "PatientDischarged": await HandleDischarge(json, producer, ct); break;
            default: throw new InvalidOperationException($"Unknown event type '{type}'");
        }
    }

    private async Task HandleAdmission(string json, IProducer<string, string> producer, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<AdmissionCreated>(json)
                  ?? throw new InvalidOperationException("Could not read event");

        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HospitalDashboardContext>();

        if (await db.Admissions.AnyAsync(a => a.Id == evt.AdmissionId, ct))
        {
            log.LogInformation("Already processed {Id}, skipping", evt.AdmissionId);
            return;
        }

        var bed = await db.Beds.FindAsync(new object[] { evt.BedId }, ct)
                  ?? throw new InvalidOperationException($"Bed {evt.BedId} not found");
        if (bed.Status != "Available")
            throw new InvalidOperationException($"Bed is {bed.Status}, not Available");

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.Admissions.Add(new Admission
        {
            Id = evt.AdmissionId,
            PatientId = evt.PatientId,
            BedId = evt.BedId,
            StaffId = evt.StaffId,
            AdmittedAt = evt.AdmittedAt,
            Status = "Admitted"
        });
        bed.Status = "Occupied";
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        await Publish(producer, bed.Id, "Occupied", evt.AdmissionId, ct);
        log.LogInformation("Admission {Id} saved, bed {Bed} now Occupied", evt.AdmissionId, bed.BedNumber);
    }

    private async Task HandleDischarge(string json, IProducer<string, string> producer, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<PatientDischarged>(json)
                  ?? throw new InvalidOperationException("Could not read event");

        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HospitalDashboardContext>();

        var admission = await db.Admissions.FindAsync(new object[] { evt.AdmissionId }, ct)
                        ?? throw new InvalidOperationException($"Admission {evt.AdmissionId} not found");

        if (admission.DischargedAt is not null)
        {
            log.LogInformation("Admission {Id} already discharged, skipping", evt.AdmissionId);
            return;
        }

        var bed = await db.Beds.FindAsync(new object[] { admission.BedId }, ct)
                  ?? throw new InvalidOperationException($"Bed {admission.BedId} not found");

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        admission.DischargedAt = evt.DischargedAt;
        admission.Status = "Discharged";
        bed.Status = "Cleaning";
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        await Publish(producer, bed.Id, "Cleaning", admission.Id, ct);
        log.LogInformation("Admission {Id} discharged, bed {Bed} now Cleaning", admission.Id, bed.BedNumber);
    }

    private static Task Publish(IProducer<string, string> producer, Guid bedId, string status, Guid admissionId, CancellationToken ct)
    {
        var done = new BedStatusChanged(Guid.NewGuid(), "BedStatusChanged", bedId, status, admissionId, DateTime.UtcNow);
        return producer.ProduceAsync("bed-events.processed",
            new Message<string, string> { Key = bedId.ToString(), Value = JsonSerializer.Serialize(done) }, ct);
    }
}