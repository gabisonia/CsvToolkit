using System.Data;
using CsvToolkit.Core;

namespace CsvToolkit.Sample.Demos;

public static class DataReaderApiDemo
{
    public static void Run(string csvPath, CsvOptions options)
    {
        Console.WriteLine("\n[8] ADO.NET API (AsDataReader)");

        using var stream = File.OpenRead(csvPath);
        using var reader = new CsvReader(stream, options);
        using var dataReader = reader.AsDataReader();

        var table = new DataTable();
        table.Load(dataReader);

        Console.WriteLine(
            $"table: Rows={table.Rows.Count} Columns={table.Columns.Count} FirstName={table.Rows[0]["full_name"]}");
    }
}