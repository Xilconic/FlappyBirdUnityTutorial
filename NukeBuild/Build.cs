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
    private static AbsolutePath _unityGameSrcDirecetory = RootDirectory / "GameMakersToolkitFlappyBirdTutorial";

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
        .Executes(() =>
        {
            const string versionText = "0.4.0-beta"; // TODO: Make this a required argument?

            var projectSettingsFile = _unityGameSrcDirecetory / "ProjectSettings" / "ProjectSettings.asset";
            // Match to `  bundleVersion: x.y.z-alpha` or `  bundleVersion: x.y.z-beta` or `  bundleVersion: x.y.z`:
            Regex projectSettingsVersionRegex = GetProjectSettingsVersionRegex();
            UpdateVersionTextInFileUsingRegex(versionText, projectSettingsFile, projectSettingsVersionRegex, "bundleVersion: <version>");
                
            var mainMenuSceneFile = _unityGameSrcDirecetory / "Assets" / "Scenes" / "MainMenuScene.unity";
            // Match to `  m_Text: 'Version: x.y.z-alpha'` or `  m_Text: 'Version: x.y.z-beta'` or `  m_Text: 'Version: x.y.z'`:
            var mainMenuSceneVersionTextValueRegex = GetMainMenuSceneVersionTextValueRegex();
            UpdateVersionTextInFileUsingRegex(versionText, mainMenuSceneFile, mainMenuSceneVersionTextValueRegex, "m_Text: 'Version: <version>'");
        });

    Target BuildAppAsWin64bit => _ => _
        .Executes(() =>
        {
            var buildDirectory = RootDirectory / "Builds" / "winx64" / "GameMakersToolkitFlappyBirdTutorial.exe";
            UnityTasks.Unity(unitySettings => unitySettings
                .SetQuit(true)
                .SetBatchMode(true)
                .SetBuildTarget(UnityBuildTarget.StandaloneWindows64)
                .SetBuildWindows64Player(buildDirectory)
                .SetProjectPath(_unityGameSrcDirecetory)
                .SetLogFile(RootDirectory / "NukeBuild" / "unity.log")
            );
        });

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
}
