// Run with: dotnet script FixMigrationHistory.csx
// Or use dotnet-script: dotnet tool install -g dotnet-script
#r "nuget: Npgsql, 9.0.3"

using Npgsql;

var connStr = "Host=dpg-d6q19175gffc73ducn0g-a.oregon-postgres.render.com;Port=5432;Database=abpgroup;Username=abpgroup_user;Password=Q5TpUyf1LIzmKsifececMQ2Q3er2GCdW;Ssl Mode=Require;Trust Server Certificate=true;";

using var conn = new NpgsqlConnection(connStr);
conn.Open();

var migrations = new[] {
    "20260318180433_AddProjectCodeGenFields",
    "20260318193505_AddProjectStatusMessage",
    "20260319105845_AddCodeGenSessions"
};

foreach (var m in migrations)
{
    using var cmd = new NpgsqlCommand(
        "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES (@m, @v) ON CONFLICT DO NOTHING", conn);
    cmd.Parameters.AddWithValue("m", m);
    cmd.Parameters.AddWithValue("v", "9.0.6");
    var rows = cmd.ExecuteNonQuery();
    Console.WriteLine($"{m} -> {(rows > 0 ? "inserted" : "already exists")}");
}

Console.WriteLine("Done.");
