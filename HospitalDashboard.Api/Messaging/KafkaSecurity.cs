using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

// Enables SASL_SSL when Kafka:SaslPassword is configured (Azure Event Hubs Kafka endpoint:
// username "$ConnectionString", password = the Event Hubs connection string).
// With no password set (local Docker Kafka) the config is left unchanged.
public static class KafkaSecurity
{
    public static T WithSasl<T>(this T cfg, IConfiguration config) where T : ClientConfig
    {
        var password = config["Kafka:SaslPassword"];
        if (string.IsNullOrWhiteSpace(password)) return cfg;

        cfg.SecurityProtocol = SecurityProtocol.SaslSsl;
        cfg.SaslMechanism = SaslMechanism.Plain;
        cfg.SaslUsername = config["Kafka:SaslUsername"] ?? "$ConnectionString";
        cfg.SaslPassword = password;
        return cfg;
    }
}
