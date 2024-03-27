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

partial class Build : NukeBuild
{
    private const string AppName = "GameMakersToolkitFlappyBirdTutorial";

    private static readonly AbsolutePath _unityGameSrcDirectory = RootDirectory / "GameMakersToolkitFlappyBirdTutorial";
    private static readonly AbsolutePath _buildsDirectory = RootDirectory / "Builds";
    private static readonly AbsolutePath _winx64BuildDirectory = _buildsDirectory / "winx64";
    private static readonly AbsolutePath _winx86BuildDirectory = _buildsDirectory / "winx86";

    [Parameter("The version of the build taking place; Needs to be formatted in x.y.z, optionally post-fixes with -alpha or -beta")] 
    readonly string Version;

    [GitRepository] readonly GitRepository Repository;

    private AbsolutePath _targetAppFileLocation;
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
            _targetAppFileLocation = _winx64BuildDirectory / _releaseVersion / $"{AppName}.exe";
            BuildWin64BitInTargetDirectory(_targetAppFileLocation, true);
            
        });

    Target BuildWin32BitReleaseBinaries => _ => _
        .DependsOn(SetVersion)
        .Unlisted()
        .Executes(() =>
        {
            _releaseVersion = $"{AppName}_winx86_{Version}";
            _targetAppFileLocation = _winx86BuildDirectory / _releaseVersion / $"{AppName}.exe";
            BuildWin32BitInTargetDirectory(_targetAppFileLocation, true);

        });

    Target BuildWin64BitReleaseZipFile => _ => _
        .DependsOn(BuildWin64BitReleaseBinaries)
        .Executes(() =>
        {
            AbsolutePath releaseBinariesDirectory = _targetAppFileLocation.Parent;
            releaseBinariesDirectory.ZipTo(
                releaseBinariesDirectory + ".zip", 
                compressionLevel: CompressionLevel.SmallestSize
            );
        });

    Target BuildWin32BitReleaseZipFile => _ => _
        .DependsOn(BuildWin32BitReleaseBinaries)
        .Executes(() =>
        {
            AbsolutePath releaseBinariesDirectory = _targetAppFileLocation.Parent;
            releaseBinariesDirectory.ZipTo(
                releaseBinariesDirectory + ".zip",
                compressionLevel: CompressionLevel.SmallestSize
            );
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

    [GeneratedRegex("^\\s+bundleVersion:\\s+(?<currentVersion>\\d+\\.\\d+\\.\\d+(-(alpha|beta))?)$")]
    private static partial Regex GetProjectSettingsVersionRegex();
    [GeneratedRegex("^\\s+m_Text:\\s+'Version: (?<currentVersion>\\d+\\.\\d+\\.\\d+(-(alpha|beta))?)'$")]
    private static partial Regex GetMainMenuSceneVersionTextValueRegex();
    [GeneratedRegex("^\\d+\\.\\d+\\.\\d+(-(alpha|beta))?$")]
    private static partial Regex GetValidVersionRegex();
}
