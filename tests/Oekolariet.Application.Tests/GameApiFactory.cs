using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Oekolariet.Api.Tests;

internal sealed class GameApiFactory(string containerConnectionString)
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
                ["ConnectionStrings:Oekolariet"] = connection.ConnectionString
            });
        });

        // Development startup applies the migrations and seeds the exhibition.
        builder.UseEnvironment("Development");

        return base.CreateHost(builder);
    }
}
