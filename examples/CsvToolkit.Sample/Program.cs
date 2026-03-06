using CsvToolkit.Sample.Demos;
using CsvToolkit.Sample.Infrastructure;

var paths = SamplePaths.FromBaseDirectory(AppContext.BaseDirectory);
Directory.CreateDirectory(paths.OutputDirectory);

Console.WriteLine("=== CsvToolkit.Core Sample ===");
Console.WriteLine($"Data directory: {paths.DataDirectory}");

var options = CsvOptionsFactory.Create();

RowApiDemo.Run(paths.PeoplePath, options);
DictionaryApiDemo.Run(paths.PeoplePath, options);
DynamicApiDemo.Run(paths.PeoplePath, options);
EnumerableApiDemo.Run();
AttributeRecordApiDemo.Run(paths.PeoplePath, options);
AdvancedAttributeApiDemo.Run();
FluentMapApiDemo.Run(paths.EmployeesPath, options);
DataReaderApiDemo.Run(paths.PeoplePath, options);
WriteApiDemo.Run(paths.PeopleExportPath, options);
await AsyncApiDemo.RunAsync(paths.PeoplePath, paths.AsyncExportPath, options);

Console.WriteLine("=== Sample Completed ===");