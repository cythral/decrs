using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Decrs.Artifacts;
using Decrs.Cicd.Utils;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Decrs.Cicd.BuildDriver
{
    /// <inheritdoc />
    public class Host : IHost
    {
        private static readonly string ConfigFile = ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "cicd/config.yml";
        private static readonly string IntermediateOutputDirectory = ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "obj/Cicd.Driver/";
        private static readonly string CicdOutputDirectory = ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "bin/Cicd/";
        private static readonly string OutputsFile = IntermediateOutputDirectory + "cdk.outputs.json";
        private readonly CommandLineOptions options;
        private readonly EcrUtils ecrUtils;
        private readonly IHostApplicationLifetime lifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Host" /> class.
        /// </summary>
        /// <param name="ecrUtils">Utilities for using ECR.</param>
        /// <param name="options">Command line options.</param>
        /// <param name="lifetime">Service that controls the application lifetime.</param>
        /// <param name="serviceProvider">Object that provides access to the program's services.</param>
        public Host(
            EcrUtils ecrUtils,
            IOptions<CommandLineOptions> options,
            IHostApplicationLifetime lifetime,
            IServiceProvider serviceProvider
        )
        {
            this.options = options.Value;
            this.ecrUtils = ecrUtils;
            this.lifetime = lifetime;
            Services = serviceProvider;
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArtifactsStackOutputs outputs = null!;

            await Step("Deploying Artifacts Stack", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                outputs = await ArtifactsStack.Deploy(cancellationToken);
            });

            await Step("Logging into ECR", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ecrUtils.PublicDockerLogin(cancellationToken);
            });

            await Step("Building Docker Image", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var tag = options.Version.Replace('/', '-');
                var buildOptions = new Dictionary<string, object>
                {
                    ["--tag"] = $"{outputs.ImageRepositoryUri}:{tag}",
                    ["--file"] = $"{ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory}Dockerfile",
                    ["--platform"] = "linux/arm64",
                    ["--push"] = options.Push,
                };

                if (!options.NoCache)
                {
                    var cache = "type=gha,scope=decrs";
                    buildOptions["--cache-from"] = cache;
                    buildOptions["--cache-to"] = cache;
                }

                var command = new Command(
                    command: "docker buildx build",
                    options: buildOptions,
                    arguments: new[] { ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory }
                );

                await command.RunOrThrowError(
                    errorMessage: "Failed to build Docker Image.",
                    cancellationToken: cancellationToken
                );
            });

            await Step("[Cleanup] Logout of ECR", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var publicEcrLogoutCommand = new Command("docker logout", arguments: new[] { "public.ecr.aws" });
                await publicEcrLogoutCommand.RunOrThrowError("Could not logout of ECR.");
            });

            lifetime.StopApplication();
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private static async Task Step(string title, Func<Task> action)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{title} ==========\n");
            Console.ResetColor();

            await action();
        }
    }
}
