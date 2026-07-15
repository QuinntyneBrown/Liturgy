using Microsoft.Data.SqlClient;

namespace Liturgy.IntegrationTests.Support;

/// <summary>
/// Resolves the SQL Server instance the integration tests run against. Defaults to
/// the local SQL Express instance (LocalDB's native provider is x64-only and cannot
/// be loaded from an arm64 test host); override with the LITURGY_TEST_SQLSERVER
/// environment variable to point at a different instance or a CI server.
/// </summary>
internal static class AcceptanceSqlServer
{
    private const string ConnectionStringEnvironmentVariable = "LITURGY_TEST_SQLSERVER";
    private const string DefaultConnectionString =
        @"Server=.\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True";

    public static string MasterConnectionString => ForDatabase("master");

    public static string ForDatabase(string databaseName)
    {
        var builder = new SqlConnectionStringBuilder(BaseConnectionString)
        {
            InitialCatalog = databaseName
        };

        return builder.ConnectionString;
    }

    private static string BaseConnectionString =>
        Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable)
        ?? DefaultConnectionString;
}
