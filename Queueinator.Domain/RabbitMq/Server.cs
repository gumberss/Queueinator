using System;
using System.Collections.Generic;

namespace Queueinator.Domain.RabbitMq
{
    public class Server
    {
        public Server()
        {
            Hosts = new List<VirtualHost>();
        }

        public string Name { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public List<VirtualHost> Hosts { get; set; }

        public void AddHosts(List<VirtualHost> hosts)
        {
            Hosts.AddRange(hosts);
        }
    }
}
