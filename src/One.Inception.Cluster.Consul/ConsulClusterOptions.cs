namespace One.Inception.Persistence.Cassandra
{
    public class ConsulClusterOptions
    {
        public string Address { get; set; } = "consul.local.com";
        public int Port { get; set; } = 8500;
    }
}
