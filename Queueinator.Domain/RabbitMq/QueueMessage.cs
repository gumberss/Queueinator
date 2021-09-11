using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Queueinator.Domain.RabbitMq
{
    public class QueueMessage
    {
        [JsonPropertyName("payload_bytes")]
        public long Bytes { get; set; }

        [JsonPropertyName("payload")]
        public String Payload { get; set; }

        [JsonPropertyName("exchange")]
        public String Exchange { get; set; }

        [JsonPropertyName("message_count")]
        public int MessageCount { get; set; }

        [JsonPropertyName("properties")]
        public MessageProperties Properties { get; set; }
    }

    public class MessageProperties
    {
        [JsonPropertyName("message_id")]
        public Guid Id { get; set; }

        [JsonPropertyName("delivery_mode")]
        public int DeliveryMode { get; set; }

        [JsonPropertyName("headers")]
        public object Headers { get; set; }
    }
}
