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

        [JsonPropertyName("routing_key")]
        public String RoutingKey { get; set; }

        [JsonPropertyName("payload_encoding")]
        public String Encoding { get; set; }

        public QueueMessage Clone()
        {
            return new QueueMessage
            {
                Bytes = Bytes,
                Payload = Payload,
                Exchange = Exchange,
                MessageCount = MessageCount,
                Properties = new MessageProperties
                {
                    DeliveryMode = Properties.DeliveryMode,
                    Headers = Properties.Headers,
                    Id = Guid.NewGuid(),
                },
                RoutingKey = RoutingKey,
                Encoding = Encoding,
            };
        }
    }

    public class MessageProperties
    {
        [JsonPropertyName("message_id")]
        public Guid Id { get; set; }

        [JsonPropertyName("delivery_mode")]
        public byte DeliveryMode { get; set; }

        [JsonPropertyName("headers")]
        public object Headers { get; set; }
    }
}
