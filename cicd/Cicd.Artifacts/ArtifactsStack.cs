using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon.CDK;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.IAM;

using Decrs.Cicd.Utils;

namespace Decrs.Artifacts
{
    /// <summary>
    /// Stack that contains repositories for storing artifacts.
    /// </summary>
    public class ArtifactsStack : Stack
    {
        private const string Name = "decrs-cicd";
        private static readonly string OutputDirectory = ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "obj/Cicd.Artifacts/cdk.out";

        private static readonly App App = new(new AppProps
        {
            Outdir = OutputDirectory,
        });

        private static readonly StackProps Props = new()
        {
            Synthesizer = new BootstraplessSynthesizer(new BootstraplessSynthesizerProps()),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactsStack" /> class.
        /// </summary>
        public ArtifactsStack()
            : base(App, Name, Props)
        {
            AddRepository();
        }

        /// <summary>
        /// Deploys the artifacts stack.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The stack outputs.</returns>
        public static async Task<ArtifactsStackOutputs> Deploy(CancellationToken cancellationToken)
        {
            _ = new ArtifactsStack();
            var result = App.Synth();
            var deployer = new StackDeployer();
            var templateFile = result.Stacks.First().TemplateFullPath;
            var templateBody = await File.ReadAllTextAsync(templateFile, cancellationToken);

            var context = new DeployContext { StackName = Name, TemplateBody = templateBody };
            var outputs = await deployer.Deploy(context, cancellationToken);

            return new ArtifactsStackOutputs
            {
                ImageRepositoryUri = outputs[nameof(ArtifactsStackOutputs.ImageRepositoryUri)],
            };
        }

        /// <summary>
        /// Adds the repository to the artifacts stack.
        /// </summary>
        public void AddRepository()
        {
            var policy = new PolicyDocument();
            var repository = new CfnPublicRepository(this, "DockerRepo", new CfnPublicRepositoryProps
            {
                RepositoryName = "decrs",
                RepositoryPolicyText = policy,
            });

            var tokenStatement = new PolicyStatement() { Effect = Effect.ALLOW };
            tokenStatement.AddAwsAccountPrincipal(Fn.Ref("AWS::AccountId"));
            tokenStatement.AddActions(
                "ecr-public:GetAuthorizationToken",
                "sts:GetServiceBearerToken"
            );

            var pullStatement = new PolicyStatement() { Effect = Effect.ALLOW };
            pullStatement.AddAnyPrincipal();
            pullStatement.AddActions(
                "ecr-public:GetDownloadUrlForLayer",
                "ecr-public:BatchGetImage",
                "ecr-public:BatchCheckLayerAvailability",
                "ecr-public:ListImages"
            );

            policy.AddStatements(
                tokenStatement,
                pullStatement
            );

            repository.ApplyRemovalPolicy(RemovalPolicy.DESTROY);
            _ = new CfnOutput(this, "ImageRepositoryUri", new CfnOutputProps
            {
                Value = $"public.ecr.aws/cythral/{repository.Ref}",
                Description = "URI of the container image repository for Decrs.",
            });
        }
    }
}
