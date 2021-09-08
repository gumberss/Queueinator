using System;
using System.Text.Json.Serialization;

namespace Queueinator.Domain.RabbitMq
{
    public class VirtualHost
    {
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public String Name { get; set; }

        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public String Description { get; set; }
        
        public string Server { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string VHost { get; set; }
    }
}
