namespace Decrs.Cicd.BuildDriver
{
    /// <summary>
    /// Options presented on the command line.
    /// </summary>
    public class CommandLineOptions
    {
        /// <summary>
        /// Gets or sets the version to build.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the image should be pushed or not.
        /// </summary>
        public bool Push { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to not use the github actions cache for docker image layers.
        /// </summary>
        public bool NoCache { get; set; } = false;
    }
}
