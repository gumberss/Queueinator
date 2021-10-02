using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Queueinator.Domain.RabbitMq
{
    public class ExchangeBindingsGrouper
    {
        public ExchangeBindingsGrouper(List<ExchangeBinding> fromExchange, List<ExchangeBinding> toExchange)
        {
            FromExchange = fromExchange;
            ToExchange = toExchange;
        }

        public List<ExchangeBinding> FromExchange { get; set; }

        public List<ExchangeBinding> ToExchange { get; set; }
    }

    public class ExchangeBinding
    {
        [JsonPropertyName("source")]
        public String Source { get; set; }

        [JsonPropertyName("vhost")]
        public String Host { get; set; }

        [JsonPropertyName("destination")]
        public String Destination { get; set; }

        [JsonPropertyName("destination_type")]
        public String DestinationType { get; set; }

        [JsonPropertyName("routing_key")]
        public String RoutingKey { get; set; }

        [JsonPropertyName("arguments")]
        public object Arguments { get; set; }

        [JsonPropertyName("properties_key")]
        public String PropertiesKey { get; set; }
    }
}
