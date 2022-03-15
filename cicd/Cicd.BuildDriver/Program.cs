using System.Collections.Generic;

using Decrs.Cicd.BuildDriver;
using Decrs.Cicd.Utils;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#pragma warning disable SA1516

await Microsoft.Extensions.Hosting.Host
.CreateDefaultBuilder()
.ConfigureAppConfiguration(configure =>
{
    configure.AddCommandLine(args, new Dictionary<string, string>
    {
        ["--version"] = "CommandLineOptions:Version",
        ["--push"] = "CommandLineOptions:Push",
        ["--no-cache"] = "CommandLineOptions:NoCache",
    });
})
.ConfigureServices((context, services) =>
{
    services.Configure<CommandLineOptions>(context.Configuration.GetSection("CommandLineOptions"));
    services.AddSingleton<IHost, Decrs.Cicd.BuildDriver.Host>();
    services.AddSingleton<EcrUtils>();
    services.AddSingleton<DockerHubUtils>();
})
.UseConsoleLifetime()
.Build()
.RunAsync();
