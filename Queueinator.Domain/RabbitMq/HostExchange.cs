using System;
using System.Text.Json.Serialization;

namespace Queueinator.Domain.RabbitMq
{
    public class HostExchange : INode
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("auto_delete")]
        public bool AutoDelete { get; set; }

        [JsonPropertyName("durable")]
        public bool Durable { get; set; }

        [JsonPropertyName("internal")]
        public bool Internal { get; set; }

        [JsonPropertyName("type")]
        public String Type { get; set; }

        [JsonPropertyName("user_who_performed_action")]
        public String CreatedBy { get; set; }

        [JsonPropertyName("vhost")]
        public String Host { get; set; }
    }
}
