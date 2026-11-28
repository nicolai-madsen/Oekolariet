using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Oekolariet.Api.Tests;

// Starts the real API (Program.cs) for tests only, with a test database (Testcontainers) and a test JWT key.
// Nothing here is used when the real API runs.
internal sealed class ApiFactory(string containerConnectionString)
    : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var connection = new SqlConnectionStringBuilder(containerConnectionString)
        {
            InitialCatalog = "OekolarietApiTests"
        };

        // Supply the test connection before Program.cs registers the DbContext.
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Oekolariet"] = connection.ConnectionString,
                // TEST ONLY: a fixed key so the API can start on every PC and in CI.
                // The real key is never in Git (user-secrets in development, .env in Docker).
                ["Jwt:Key"] = "test-signing-key-only-for-api-tests-1234567890"
            });
        });

        // Development startup applies the migrations and seeds the exhibition.
        builder.UseEnvironment("Development");

        return base.CreateHost(builder);
    }
}
