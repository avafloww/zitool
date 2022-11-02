using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using ZiTool.Commands;
using ZiTool.Thaliak;

var services = new ServiceCollection()
    .AddLogging(config => { config.AddSimpleConsole(o => { o.TimestampFormat = "yyyy-MM-dd HH:mm:ss "; }); });

services.AddScoped<ThaliakClient>();

using var registrar = new DependencyInjectionRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.ValidateExamples();

    config.AddCommand<InspectCommand>("inspect")
        .WithDescription("Inspects and returns information about a ZiPatch file.")
        .WithExample(new[] {"inspect", "D2022.04.20.0000.0000.patch"})
        .WithExample(new[] {"inspect", "D2022.04.20.0000.0000.patch", "repository"})
        .WithExample(new[] {"inspect", "D2022.04.20.0000.0000.patch", "type"})
        .WithExample(new[] {"inspect", "D2022.04.20.0000.0000.patch", "minor"});

    config.AddCommand<ApplyCommand>("apply")
        .WithDescription("Applies a ZiPatch file to a directory with given options.")
        .WithExample(new[] {"apply", "D2022.04.20.0000.0000.patch", "/path/to/ffxiv"});
});

return await app.RunAsync(args);
