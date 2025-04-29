using System;
using System.Linq;
using One.Inception.Cluster.Consul;
using One.Inception.Cluster.Job;
using One.Inception.Discoveries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace One.Inception.Persistence.Cassandra
{
    public class ConsulClusterDiscovery : DiscoveryBase<IInceptionJob<object>>
    {
        protected override DiscoveryResult<IInceptionJob<object>> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<IInceptionJob<object>>(Enumerable.Empty<DiscoveredModel>(), AddServices);
        }

        private void AddServices(IServiceCollection services)
        {
            services.AddOptions<ConsulClusterOptions, ConsulClusterOptionsProvider>();

            services.AddHttpClient<IInceptionJobRunner, InceptionJobRunner>("inception", (provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<ConsulClusterOptions>>().Value;
                var builder = new UriBuilder(options.Address);
                builder.Port = options.Port;

                client.BaseAddress = builder.Uri;

                //var authorization = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{gateway.ApiClient.ApiUsername}:{gateway.ApiClient.ApiPassword}"));
                //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authorization);
            });
        }
    }
}
