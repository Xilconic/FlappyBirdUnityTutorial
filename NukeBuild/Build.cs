using System;
using System.Linq;
using System.IO;
using System.IO.Compression;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Unity;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using System.Text.RegularExpressions;
using Serilog;
using Nuke.Common.Git;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitHub;
using Octokit;
using _build;
using System.Collections.Generic;
using System.Net.Mime;

partial class Build : NukeBuild
{
    private const string AppName = "GameMakersToolkitFlappyBirdTutorial";

    private static readonly AbsolutePath _unityGameSrcDirectory = RootDirectory / "GameMakersToolkitFlappyBirdTutorial";
    private static readonly AbsolutePath _buildsDirectory = RootDirectory / "Builds";
    private static readonly AbsolutePath _winx64BuildDirectory = _buildsDirectory / "winx64";
    private static readonly AbsolutePath _winx86BuildDirectory = _buildsDirectory / "winx86";

    [GitRepository] readonly GitRepository Repository;

    [Parameter("The version of the build taking place; Needs to be formatted in x.y.z, optionally post-fixes with -alpha or -beta")] 
    readonly string Version;

    [Parameter("The password for Github"), Secret] 
    readonly string GitHubPassword;

    [Parameter("The username for Github")]
    readonly string GitHubUser;

    [Parameter("The 'Personal Access Token' used by the Build scripts for creating releases"), Secret]
    readonly string GitHubPersonalAccessToken;

    private readonly Dictionary<ReleaseTypes, AbsolutePath> _targetAppFileLocations = new();
    private string _releaseVersion;

    public static int Main() => Execute<Build>();

    Target SetVersion => _ => _
        .Requires(() => Version)
        .Unlisted()
        .Executes(() =>
        {
            Assert.True(GetValidVersionRegex().IsMatch(Version), "The 'Version' Parameter needs to be in the 'SemVer' fomat of x.y.z, with optionally -alpha or -beta postfix.");

            var projectSettingsFile = _unityGameSrcDirectory / "ProjectSettings" / "ProjectSettings.asset";
            // Match to `  bundleVersion: x.y.z-alpha` or `  bundleVersion: x.y.z-beta` or `  bundleVersion: x.y.z`:
            Regex projectSettingsVersionRegex = GetProjectSettingsVersionRegex();
            UpdateVersionTextInFileUsingRegex(Version!, projectSettingsFile, projectSettingsVersionRegex, "bundleVersion: <version>");
                
            var mainMenuSceneFile = _unityGameSrcDirectory / "Assets" / "Scenes" / "MainMenuScene.unity";
            // Match to `  m_Text: 'Version: x.y.z-alpha'` or `  m_Text: 'Version: x.y.z-beta'` or `  m_Text: 'Version: x.y.z'`:
            var mainMenuSceneVersionTextValueRegex = GetMainMenuSceneVersionTextValueRegex();
            UpdateVersionTextInFileUsingRegex(Version!, mainMenuSceneFile, mainMenuSceneVersionTextValueRegex, "m_Text: 'Version: <version>'");
        });

    Target BuildAppAsWin64bit => _ => _
        .Executes(() =>
        {
            var targetAppFileLocation = _winx64BuildDirectory / $"{AppName}.exe";
            BuildWin64BitInTargetDirectory(targetAppFileLocation);
        });

    Target BuildAppAsWin32bit => _ => _
        .Executes(() =>
        {
            var targetAppFileLocation = _winx86BuildDirectory / $"{AppName}.exe";
            BuildWin32BitInTargetDirectory(targetAppFileLocation);
        });

    Target BuildWin64BitReleaseBinaries => _ => _
        .DependsOn(SetVersion)
        .Unlisted()
        .Executes(() =>
        {
            _releaseVersion = $"{AppName}_winx64_{Version}";
            _targetAppFileLocations[ReleaseTypes.Windows64Bit] = _winx64BuildDirectory / _releaseVersion / $"{AppName}.exe";
            BuildWin64BitInTargetDirectory(_targetAppFileLocations[ReleaseTypes.Windows64Bit], true);
            
        });

    Target BuildWin32BitReleaseBinaries => _ => _
        .DependsOn(SetVersion)
        .Unlisted()
        .Executes(() =>
        {
            _releaseVersion = $"{AppName}_winx86_{Version}";
            _targetAppFileLocations[ReleaseTypes.Windows32Bit] = _winx86BuildDirectory / _releaseVersion / $"{AppName}.exe";
            BuildWin32BitInTargetDirectory(_targetAppFileLocations[ReleaseTypes.Windows32Bit], true);

        });

    Target BuildWin64BitReleaseZipFile => _ => _
        .DependsOn(BuildWin64BitReleaseBinaries)
        .Executes(() =>
        {
            ZipRelease(ReleaseTypes.Windows64Bit);
        });

    Target BuildWin32BitReleaseZipFile => _ => _
        .DependsOn(BuildWin32BitReleaseBinaries)
        .Executes(() =>
        {
            ZipRelease(ReleaseTypes.Windows32Bit);
        });

    Target CreateReleaseCommitAndPush => _ => _
        .DependsOn(SetVersion)
        .Requires(() => Repository.IsOnMainBranch()) // This project uses Trunk Based Development
        .Executes(() =>
        {
            // Stage all side-effects of SetVersion Target:
            GitTasks.Git("add .");

            // Create release commit:
            GitTasks.Git($"commit -m \"Build {Version}\"");

            // Tag release commit:
            GitTasks.Git($"tag -a {Version} -m \"Tag build {Version}\"");

            // Push to origin:
            GitTasks.Git("push origin --tags main:main");
        });

    Target CreateReleaseOnGithub => _ => _
        .Requires(() => Version)
        //.Requires(() => GitHubUser)
        //.Requires(() => GitHubPassword)
        .Requires(() => GitHubPersonalAccessToken)
        .DependsOn(CreateReleaseCommitAndPush)
        .DependsOn(BuildWin64BitReleaseZipFile)
        .DependsOn(BuildWin32BitReleaseZipFile)
        .Executes(async () =>
        {
            string branchName = await GitHubTasks.GetDefaultBranch(Repository);
            var productInformation = new ProductHeaderValue($"{AppName}-NukeBuildScript");
            GitHubTasks.GitHubClient = new GitHubClient(productInformation)
            {
                //Credentials = new Credentials(GitHubUser, GitHubPassword)
                Credentials = new Credentials(GitHubPersonalAccessToken)
            };

            var releaseData = new NewRelease(Version)
            {
                Prerelease = Version.EndsWith("alpha") || Version.EndsWith("beta"),
                Body = "TODO"
            };
            try
            {
                Release release = await GitHubTasks.GitHubClient.Repository.Release
                    .Create(Repository.GetGitHubOwner(), Repository.GetGitHubName(), releaseData);

                foreach (ReleaseTypes releaseType in _targetAppFileLocations.Keys)
                {
                    AbsolutePath releaseArtifact = GetReleaseZipFilePath(releaseType);
                    var upload = new ReleaseAssetUpload
                    {
                        ContentType = MediaTypeNames.Application.Zip,
                        FileName = Path.GetFileName(releaseArtifact),
                        RawData = File.OpenRead(releaseArtifact)
                    };
                    await GitHubTasks.GitHubClient.Repository.Release
                                    .UploadAsset(release, upload);
                }
            }
            catch (ApiException e)
            {
                Log.Debug("{@ExceptionData}", e);
                Log.Error(e, "Failure during release creation towards Github");
            }
            
        });

    private static void BuildWin64BitInTargetDirectory(AbsolutePath targetAppFileLocation, bool isReleaseBuild = false)
    {
        BuildInTargetDirectory(
            unitySettings => _build.UnitySettingsExtensions.SetWindows64BitBuildTarget(unitySettings, targetAppFileLocation),
            isReleaseBuild
        );
    }

    private static void BuildWin32BitInTargetDirectory(AbsolutePath targetAppFileLocation, bool isReleaseBuild = false)
    {
        BuildInTargetDirectory(
            unitySettings => _build.UnitySettingsExtensions.SetWindows32BitBuildTarget(unitySettings, targetAppFileLocation), 
            isReleaseBuild
        );
    }

    private static void BuildInTargetDirectory(
        Func<UnitySettings, UnitySettings> setBuildTarget,
        bool isReleaseBuild = false)
    {
        UnityTasks.Unity(unitySettings =>
        {
            UnitySettings settings = unitySettings
                .EnableQuit()
                .EnableBatchMode()
                .SetProjectPath(_unityGameSrcDirectory)
                .SetLogFile(RootDirectory / "NukeBuild" / "unity.log");
            settings = setBuildTarget(settings);
            if (isReleaseBuild)
            {
                settings.AddCustomArguments("-releaseCodeOptimization");
            }
            return settings;
        });
    }

    private static void UpdateVersionTextInFileUsingRegex(string versionText, AbsolutePath fileToBeUpdated, Regex regex, string expectedPattern)
    {
        Assert.FileExists(fileToBeUpdated);
        string[] fileLines = fileToBeUpdated.ReadAllLines();

        bool updatedVersion = false;
        for (int i = 0; i < fileLines.Length; i++)
        {
            var line = fileLines[i];
            var match = regex.Match(line);
            if (match.Success)
            {
                Group currentVersionCaptureGroup = match.Groups["currentVersion"];
                fileLines[i] = line.Replace(currentVersionCaptureGroup.Value, versionText);
                updatedVersion = true;
                break;
            }
        }
        if (!updatedVersion)
        {
            Assert.Fail($"\"{expectedPattern}\" pattern missing from '{fileToBeUpdated}'.");
        }
        fileToBeUpdated.WriteAllLines(fileLines);
        Log.Debug("Successfully updated {File} to {NewVersion}", fileToBeUpdated.ToString(), versionText);
    }

    private void ZipRelease(ReleaseTypes release)
    {
        AbsolutePath archiveFile = GetReleaseZipFilePath(release);
        AbsolutePath releaseDirectory = _targetAppFileLocations[release].Parent;
        releaseDirectory.ZipTo(
            archiveFile,
            compressionLevel: CompressionLevel.SmallestSize
        );
    }

    private AbsolutePath GetReleaseZipFilePath(ReleaseTypes win64BitRelease) => _targetAppFileLocations[win64BitRelease].Parent + ".zip";

    [GeneratedRegex("^\\s+bundleVersion:\\s+(?<currentVersion>\\d+\\.\\d+\\.\\d+(-(alpha|beta))?)$")]
    private static partial Regex GetProjectSettingsVersionRegex();
    [GeneratedRegex("^\\s+m_Text:\\s+'Version: (?<currentVersion>\\d+\\.\\d+\\.\\d+(-(alpha|beta))?)'$")]
    private static partial Regex GetMainMenuSceneVersionTextValueRegex();
    [GeneratedRegex("^\\d+\\.\\d+\\.\\d+(-(alpha|beta))?$")]
    private static partial Regex GetValidVersionRegex();
}
