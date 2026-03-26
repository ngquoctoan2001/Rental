using System;
using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=rentalos;Username=postgres;Password=20102001";
using var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();

Console.WriteLine("Checking public.tenants table...");
using var cmd = new NpgsqlCommand("SELECT \"Id\", \"Name\", \"SchemaName\" FROM public.tenants", conn);
using var reader = await cmd.ExecuteReaderAsync();
int count = 0;
while (await reader.ReadAsync())
{
    Console.WriteLine($"ID: {reader.GetGuid(0)}, Name: {reader.GetString(1)}, Schema: {reader.GetString(2)}");
    count++;
}
Console.WriteLine($"Total tenants: {count}");
