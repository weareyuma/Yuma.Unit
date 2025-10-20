#region Copyright & License

// Copyright © 2024 - 2025 Yuma
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using JetBrains.Annotations;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Serilog.Log;

namespace build;

[GitHubActions(
	"Continuous Integration Build",
	GitHubActionsImage.UbuntuLatest,
	FetchDepth = 0,
	OnPushBranchesIgnore = ["main", "feature/*"],
	// OnPullRequestBranches = ["main"],
	InvokedTargets = [nameof(CI)],
	EnableGitHubToken = true,
	PublishArtifacts = true,
	ImportSecrets = [nameof(YumaReleaseFeedApiKey)])]
[DotNetVerbosityMapping]
file class BuildDefinition : NukeBuild
{
	public static int Main() => Execute<BuildDefinition>(static x => x.Build);

	[NotNull]
	Target Build => td => td.DependsOn(MutationTest);

	[NotNull]
	Target CI => td => td.DependsOn(Push)
		.OnlyWhenStatic(() => IsServerBuild);

	[NotNull]
	Target Clean => td => td.Executes(() => {
		ArtifactsDirectory.CreateOrCleanDirectory();
		DotNetClean(s => s.EnableNoLogo()
			.SetConfiguration(Configuration)
			.SetProject(Solution)
			.SetVerbosity(DotNetVerbosity.minimal));
	});

	[NotNull]
	Target Restore => td => td.DependsOn(Clean)
		.Executes(() => {
			DotNetRestore(s => s //.EnableNoCache()
				.SetConfigFile(RootDirectory / "NuGet.config")
				.SetProjectFile(Solution));
			DotNetToolRestore(s => s //.EnableNoCache()
				.SetToolManifest(RootDirectory / ".config" / "dotnet-tools.json"));
		});

	[NotNull]
	Target Compile => td => td.DependsOn(Restore)
		.Executes(() => {
			Information("GitVersion.SemVer = {@SemVer}", GitVersion.SemVer);
			// Debug("GitVersion = {@GitVersion}", GitVersion.ToJson());
			DotNetBuild(s => s.EnableNoLogo()
				.EnableNoRestore()
				.SetConfiguration(Configuration)
				.SetProjectFile(Solution)
				.SetAssemblyVersion(GitVersion.AssemblySemVer)
				.SetFileVersion(GitVersion.AssemblySemFileVer)
				.SetInformationalVersion(GitVersion.InformationalVersion)
				.SetVersion(GitVersion.SemVer));
		});

	[NotNull]
	Target UnitTest => td => td.Unlisted()
		.DependsOn(Compile)
		.Produces(TestCoverageReportsDirectory / "*.html")
		.Executes(() => {
			DotNetTest(s => s.EnableNoLogo()
				.EnableNoBuild()
				// .EnableNoCache()
				.EnableNoRestore()
				.SetConfiguration(Configuration)
				.SetProjectFile(Solution)
				.SetResultsDirectory(TestResultsDirectory)
				.SetSettingsFile(RootDirectory / "coverlet.runsettings"));
			ReportGeneratorTasks.ReportGenerator(s => s.SetTargetDirectory(TestCoverageReportsDirectory)
				.AddReports(TestResultsDirectory / "**/coverage.cobertura.xml")
				.AddReportTypes(ReportTypes.lcov, ReportTypes.HtmlInline_AzurePipelines_Dark)
				.AddFileFilters("-*.g.cs"));
			string link = TestCoverageReportsDirectory / "index.html";
			Information($"Code coverage report: \e]8;;file://{link}\e\\{link}\e]8;;\e\\");
		});

	[NotNull]
	Target MutationTest => td => td.Unlisted()
		.DependsOn(UnitTest)
		.Produces(TestMutationReportsDirectory / "mutation-report.html")
		.Executes(() => {
			DotNet(workingDirectory: RootDirectory, arguments: $"stryker --output {ArtifactsDirectory} --solution ./{Solution.FileName}");
			string link = TestMutationReportsDirectory / "mutation-report.html";
			Information($"Mutation test report: \e]8;;file://{link}\e\\{link}\e]8;;\e\\");
		});

	[NotNull]
	Target Pack => td => td.DependsOn(Compile)
		.Before(UnitTest) // prevent test instrumentation contaminating packages
		.Produces(NuGetPackagesDirectory / "*.nupkg")
		.Executes(() => {
			DotNetPack(s => s.EnableNoLogo()
				.EnableNoBuild()
				// .EnableNoCache()
				.EnableNoRestore()
				.EnableContinuousIntegrationBuild()
				.SetConfiguration(Configuration)
				.SetProject(Solution)
				.SetNoDependencies(v: true)
				.SetOutputDirectory(NuGetPackagesDirectory)
				.SetVersion(GitVersion.SemVer));
		});

	[NotNull]
	Target PreviewFeedSetup => td => td.Unlisted()
		.Description("Set PushApiUrl/PushApiKey for Azure Artifacts Feed when on feature branch.")
		.OnlyWhenStatic(() => GitRepository.IsOnFeatureBranch())
		.Requires(() => YumaPreviewFeedUrl)
		.Executes(() => {
			YumaFeedApiKey = GitHubActions.Instance.Token;
			YumaFeedUrl = YumaPreviewFeedUrl;
		});

	[NotNull]
	Target ReleaseFeedSetup => td => td.Unlisted()
		.Description("Set PushApiUrl/PushApiKey for nuget.org when on main branch.")
		.OnlyWhenStatic(() => GitRepository.IsOnMainBranch())
		.Requires(() => Configuration.Equals(Configuration.Release))
		.Requires(() => YumaReleaseFeedApiKey)
		.Requires(() => YumaReleaseFeedUrl)
		.Executes(() => {
			YumaFeedApiKey = YumaReleaseFeedApiKey;
			YumaFeedUrl = YumaReleaseFeedUrl;
		});

	[NotNull]
	Target Push => td => td.DependsOn(MutationTest, Pack, PreviewFeedSetup, ReleaseFeedSetup)
		.OnlyWhenStatic(() => GitRepository.IsOnFeatureBranch())
		.Consumes(Pack)
		.Executes(() => {
			YumaFeedApiKey.NotNullOrEmpty();
			YumaFeedUrl.NotNullOrEmpty();
			NuGetPackagesDirectory.GlobFiles("*.nupkg")
				.ForEach(path => {
					Information($"Pushing NuGet package {path}");
					DotNetNuGetPush(s => s.DisableSkipDuplicate()
						.SetApiKey(YumaFeedApiKey)
						.SetSource(YumaFeedUrl)
						.SetTargetPath(path));
				});
		});

	AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

	AbsolutePath NuGetPackagesDirectory => ArtifactsDirectory / "nuget-packages";

	AbsolutePath TestCoverageReportsDirectory => ArtifactsDirectory / "test-coverage-reports";

	AbsolutePath TestMutationReportsDirectory => ArtifactsDirectory / "reports";

	AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test-results";

	[Parameter("Configuration to build: 'Debug' or 'Release'.")]
	readonly Configuration Configuration = IsLocalBuild
		? Configuration.Debug
		: Configuration.Release;

	[Required]
	[GitRepository]
	readonly GitRepository GitRepository;

	[Required]
	[GitVersion]
	readonly GitVersion GitVersion = null!;

	[Parameter("NuGet packages' preview feed URL.")]
	readonly string YumaPreviewFeedUrl = "https://nuget.pkg.github.com/weareyuma/index.json";

	[Parameter("NuGet packages' release feed API Key.")]
	[Secret]
	readonly string YumaReleaseFeedApiKey;

	[Parameter("NuGet packages' release feed URL.")]
	readonly string YumaReleaseFeedUrl = "https://api.nuget.org/v3/index.json";

	[Secret]
	string YumaFeedApiKey;

	string YumaFeedUrl;

	[Required]
	[Solution]
	[NotNull]
	readonly Solution Solution = null!;
}
