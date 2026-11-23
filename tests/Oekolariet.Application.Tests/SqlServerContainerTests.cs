using Testcontainers.MsSql;

namespace Oekolariet.Api.Tests;

public sealed class SqlServerContainerTests
{
    [Fact]
    public async Task SqlServer_CanExecuteQuery()
    {
        await using var database = new MsSqlBuilder(
            "mcr.microsoft.com/mssql/server:2022-latest").Build();

        await database.StartAsync();

        var result = await database.ExecScriptAsync("SELECT 1;");

        Assert.True(result.ExitCode == 0, result.Stderr);
    }
}
