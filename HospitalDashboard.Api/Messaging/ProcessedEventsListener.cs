using Confluent.Kafka;
using HospitalDashboard.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HospitalDashboard.Api.Messaging;

public class ProcessedEventsListener(IHubContext<AdmissionsHub> hub, IConfiguration config, ILogger<ProcessedEventsListener> log)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken ct) => Task.Run(() => Run(ct), ct);

    private async Task Run(CancellationToken ct)
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "api-signalr-broadcaster",
            AutoOffsetReset = AutoOffsetReset.Latest
        }).Build();

        consumer.Subscribe("bed-events.processed");
        log.LogInformation("API listening on bed-events.processed");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var msg = consumer.Consume(ct);
                if (msg is null) continue;
                await hub.Clients.All.SendAsync("BedStatusChanged", msg.Message.Value, ct);
                await hub.Clients.All.SendAsync("DataChanged", "admissions", ct);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { log.LogError(ex, "Broadcast failed"); }
        }

        consumer.Close();
    }
}