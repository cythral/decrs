using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;

namespace Decrs.Cicd.Utils
{
    /// <summary>
    /// Utilities for interacting with ECR.
    /// </summary>
    public class DockerHubUtils
    {
        private readonly IAmazonKeyManagementService kms = new AmazonKeyManagementServiceClient();

        /// <summary>
        /// Logs into Docker Hub.
        /// </summary>
        /// <param name="secretsFilePath">Path to the file where pipeline secrets are located.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task DockerLogin(string secretsFilePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var secretsFile = File.OpenRead(secretsFilePath);
            var secrets = await JsonSerializer.DeserializeAsync<Secrets>(secretsFile, cancellationToken: cancellationToken) ?? throw new Exception("Could not deserialize secrets.");
            var password = await Decrypt(secrets.DockerHubToken, cancellationToken);
            await Login(password, cancellationToken);
        }

        private static async Task Login(string password, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var command = new Command(
                command: "docker login",
                options: new Dictionary<string, object>
                {
                    ["--username"] = "cythral",
                    ["--password-stdin"] = true,
                }
            );

            await command.RunOrThrowError(
                errorMessage: "Failed to login to Docker Hub.",
                input: password,
                cancellationToken: cancellationToken
            );
        }

        private async Task<string> Decrypt(string ciphertext, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = new MemoryStream();
            var byteArray = Convert.FromBase64String(ciphertext);

            await stream.WriteAsync(byteArray, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            var request = new DecryptRequest { CiphertextBlob = stream };
            var response = await kms.DecryptAsync(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            using var reader = new StreamReader(response.Plaintext);
            return await reader.ReadToEndAsync();
        }
    }
}
