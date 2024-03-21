using System;
using System.Linq;
using System.IO;
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

partial class Build : NukeBuild
{
    private const string AppName = "GameMakersToolkitFlappyBirdTutorial";

    private static AbsolutePath _unityGameSrcDirectory = RootDirectory / "GameMakersToolkitFlappyBirdTutorial";
    private static AbsolutePath _winx64BuildDirectory = RootDirectory / "Builds" / "winx64";

    [Parameter] readonly string Version;

    public static int Main () => Execute<Build>();

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
        });

    Target Restore => _ => _
        .Executes(() =>
        {
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
        });

    Target SetVersion => _ => _
        .Requires(() => Version)
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

    Target BuildWin64BitReleaseBinaries => _ => _
        .DependsOn(SetVersion)
        .Executes(() =>
        {
            var releaseVersion = $"{AppName}_winx64_{Version}";
            var targetAppFileLocation = _winx64BuildDirectory / releaseVersion / $"{AppName}.exe";
            BuildWin64BitInTargetDirectory(targetAppFileLocation, true);
        });

    private static void BuildWin64BitInTargetDirectory(AbsolutePath targetAppFileLocation, bool isReleaseBuild = false)
    {
        UnityTasks.Unity(unitySettings =>
        {
            UnitySettings settings = unitySettings
                .EnableQuit()
                .EnableBatchMode()
                .SetBuildTarget(UnityBuildTarget.StandaloneWindows64)
                .SetBuildWindows64Player(targetAppFileLocation)
                .SetProjectPath(_unityGameSrcDirectory)
                .SetLogFile(RootDirectory / "NukeBuild" / "unity.log");
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
