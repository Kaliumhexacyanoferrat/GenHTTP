using GenHTTP.Playground.Samples.Ioxide;

// dotnet run -c Release --project Playground

var server = ShowcaseSample.Create();

return await server.RunAsync();
