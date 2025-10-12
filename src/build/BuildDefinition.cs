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

using System;
using JetBrains.Annotations;
using Nuke.Common.CI.AzurePipelines;
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
		.Produces(ReportsDirectory / "*.html")
		.Executes(() => {
			DotNetTest(s => s.EnableNoLogo()
				.EnableNoBuild()
				// .EnableNoCache()
				.EnableNoRestore()
				.SetConfiguration(Configuration)
				.SetProjectFile(Solution)
				.SetResultsDirectory(TestResultsDirectory)
				.SetSettingsFile(RootDirectory / "coverlet.runsettings"));
			ReportGeneratorTasks.ReportGenerator(s => s.SetTargetDirectory(ReportsDirectory)
				.AddReports(TestResultsDirectory / "**/coverage.cobertura.xml")
				.AddReportTypes(ReportTypes.lcov, ReportTypes.HtmlInline_AzurePipelines_Dark)
				.AddFileFilters("-*.g.cs"));
			string link = ReportsDirectory / "index.html";
			Information($"Code coverage report: \e]8;;file://{link}\e\\{link}\e]8;;\e\\");
		});

	[NotNull]
	Target MutationTest => td => td.Unlisted()
		.DependsOn(UnitTest)
		.Produces(ReportsDirectory / "mutation-report.html")
		.Executes(() => {
			DotNet(workingDirectory: RootDirectory, arguments: $"stryker --output {ArtifactsDirectory} --solution ./{Solution.FileName}");
			string link = ReportsDirectory / "mutation-report.html";
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
	Target AzureFeedSetup => td => td.Unlisted()
		.Description("Set PushApiUrl/PushApiKey for Azure Artifacts Feed when on feature branch.")
		.OnlyWhenDynamic(() => IsFeatureBranch)
		.Requires(() => AzureFeedApiKey)
		.Requires(() => AzureFeedUrl)
		.Executes(() => {
			PushApiKey = "AzureDevOps"; // Azure ignores API key
			PushApiUrl = AzureFeedUrl;
		});

	[NotNull]
	Target NuGetFeedSetup => td => td.Unlisted()
		.Description("Set PushApiUrl/PushApiKey for nuget.org when on main branch.")
		.OnlyWhenDynamic(() => IsMainBranch)
		.Requires(() => Configuration.Equals(Configuration.Release))
		.Requires(() => NuGetApiKey)
		.Requires(() => NuGetUrl)
		.Executes(() => {
			PushApiKey = NuGetApiKey;
			PushApiUrl = NuGetUrl;
		});

	[NotNull]
	Target Push => td => td.DependsOn(MutationTest, Pack, AzureFeedSetup, NuGetFeedSetup)
		.OnlyWhenDynamic(() => IsMainBranch || IsFeatureBranch)
		.Consumes(Pack)
		.Executes(() => {
			PushApiKey.NotNullOrEmpty();
			PushApiUrl.NotNullOrEmpty();
			NuGetPackagesDirectory.GlobFiles("*.nupkg")
				.ForEach(path => {
					Information($"Pushing NuGet package {path}");
					DotNetNuGetPush(s => s.DisableSkipDuplicate()
						.SetApiKey(PushApiKey)
						.SetSource(PushApiUrl)
						.SetTargetPath(path));
				});
		});

	[Required]
	[NotNull]
	string BranchName => GitRepository?.Branch ?? GitHubActions.Instance?.RefName ?? AzurePipelines.Instance?.SourceBranchName ?? string.Empty;

	bool IsFeatureBranch => BranchName.StartsWith("feature/", StringComparison.OrdinalIgnoreCase);

	bool IsMainBranch => BranchName.Equals("main", StringComparison.OrdinalIgnoreCase);

	AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

	AbsolutePath NuGetPackagesDirectory => ArtifactsDirectory / "nuget";

	AbsolutePath ReportsDirectory => ArtifactsDirectory / "reports";

	AbsolutePath TestResultsDirectory => ArtifactsDirectory / "tests";

	[Parameter("Azure Artifacts Feed Personal Access Token (PAT).")]
	[Secret]
	readonly string AzureFeedApiKey;

	[Parameter("Azure Artifacts Feed URL.")]
	readonly string AzureFeedUrl;

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

	[Parameter("NuGet API key.")]
	[Secret]
	readonly string NuGetApiKey;

	[Parameter("NuGet API URL.")]
	readonly string NuGetUrl;

	string PushApiUrl;

	[Secret]
	string PushApiKey;

	[Required]
	[Solution]
	[NotNull]
	readonly Solution Solution = null!;
}
