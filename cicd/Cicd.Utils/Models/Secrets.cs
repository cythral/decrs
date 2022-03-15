namespace Decrs.Cicd.Utils
{
    /// <summary>
    /// Pipeline secrets.
    /// </summary>
    public class Secrets
    {
        /// <summary>
        /// Gets or sets the encrypted docker hub token.
        /// </summary>
        public string DockerHubToken { get; set; } = string.Empty;
    }
}
