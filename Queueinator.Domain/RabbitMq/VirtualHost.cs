using System;
using System.Text.Json.Serialization;

namespace Queueinator.Domain.RabbitMq
{
    public class VirtualHost
    {
        [JsonPropertyName("name")]
        public String Name { get; set; }

        [JsonPropertyName("description")]
        public String Description { get; set; }
    }
}
