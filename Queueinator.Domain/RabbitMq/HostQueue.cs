using System;
using System.Text.Json.Serialization;

namespace Queueinator.Domain.RabbitMq
{
    public class HostQueue : INode
    {
        [JsonPropertyName("name")]
        public String Name { get; set; }

        [JsonPropertyName("state")]
        public String State { get; set; }

        [JsonPropertyName("vhost")]
        public String VirtualHostName { get; set; }

        [JsonPropertyName("type")]
        public String Type { get; set; }

        [JsonPropertyName("consumers")]
        public int ConsumersCount { get; set; }

        [JsonPropertyName("messages")]
        public int MessagesCount { get; set; }

        [JsonPropertyName("messages_ready")]
        public int MessagesReadyCount { get; set; }

        [JsonPropertyName("messages_unacknowledged")]
        public int MessagesUnacknowledgedCount { get; set; }

        [JsonPropertyName("message_bytes")]
        public long MessageBytes { get; set; }
    }
}
