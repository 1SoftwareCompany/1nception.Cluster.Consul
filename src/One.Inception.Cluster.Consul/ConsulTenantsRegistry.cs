using One.Inception.Cluster.Consul.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace One.Inception.Cluster.Consul
{
    /// <summary>
    /// Keeps the tenants of each node in its own key, inception/urn:{boundedContext}:tenants/{node}, so the nodes never overwrite each other.
    /// </summary>
    public sealed class ConsulTenantsRegistry : ITenantsRegistry
    {
        private readonly HttpClient client;
        private readonly string keyPrefix;

        public ConsulTenantsRegistry(HttpClient client, IOptions<BoundedContext> boundedContext)
        {
            this.client = client;
            keyPrefix = $"inception/urn:{boundedContext.Value.Name}:tenants/";
        }

        public async Task RegisterAsync(string node, IEnumerable<string> tenants)
        {
            StringContent content = new StringContent(JsonConvert.SerializeObject(tenants), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PutAsync($"v1/kv/{keyPrefix}{node}", content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }

        public async Task UnregisterAsync(string node)
        {
            HttpResponseMessage response = await client.DeleteAsync($"v1/kv/{keyPrefix}{node}").ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }

        public async Task<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> GetAllAsync()
        {
            HttpResponseMessage response = await client.GetAsync($"v1/kv/{keyPrefix}?recurse=true").ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return new Dictionary<string, IReadOnlyCollection<string>>();

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            List<KVResponse> keys = JsonConvert.DeserializeObject<List<KVResponse>>(json);

            return keys.ToDictionary(key => key.Key.Substring(keyPrefix.Length), key => ReadTenants(key.Value));
        }

        private static IReadOnlyCollection<string> ReadTenants(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Array.Empty<string>();

            string json = Encoding.UTF8.GetString(Convert.FromBase64String(value));

            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }
    }
}
