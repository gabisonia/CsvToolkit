using CsvToolkit.Core;
using CsvToolkit.Sample.Models;

namespace CsvToolkit.Sample.Demos;

public static class AsyncApiDemo
{
    public static async Task RunAsync(string inputPath, string outputPath, CsvOptions options)
    {
        Console.WriteLine("\n[10] Async API (GetRecordsAsync + WriteRecordsAsync)");
        var copiedRows = 0;

        await using (var input = File.OpenRead(inputPath))
        await using (var reader = new CsvReader(input, options))
        await using (var output = File.Create(outputPath))
        await using (var writer = new CsvWriter(output, options))
        {
            await writer.WriteRecordsAsync(ReadRowsAsync(), writeHeader: true);
            await writer.FlushAsync();

            async IAsyncEnumerable<AttributedPerson> ReadRowsAsync()
            {
                await foreach (var row in reader.GetRecordsAsync<AttributedPerson>())
                {
                    copiedRows++;
                    yield return row;
                }
            }
        }

        Console.WriteLine($"Async copied {copiedRows} rows to: {outputPath}");
    }
}