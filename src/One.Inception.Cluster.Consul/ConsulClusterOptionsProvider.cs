using Microsoft.Extensions.Configuration;

namespace One.Inception.Persistence.Cassandra
{
    public class ConsulClusterOptionsProvider : InceptionOptionsProviderBase<ConsulClusterOptions>
    {
        public const string SettingKey = "inception:cluster:consul";

        public ConsulClusterOptionsProvider(IConfiguration configuration) : base(configuration) { }

        public override void Configure(ConsulClusterOptions options)
        {
            configuration.GetSection(SettingKey).Bind(options);
        }
    }
}
