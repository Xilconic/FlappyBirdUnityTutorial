using System;
using System.Linq;
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

class Build : NukeBuild
{
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

    Target BuildAppAsWin64bit => _ => _
        .Executes(() =>
        {
            var buildDirectory = RootDirectory / "Builds" / "winx64" / "GameMakersToolkitFlappyBirdTutorial.exe";
            var projectDirectory = RootDirectory / "GameMakersToolkitFlappyBirdTutorial";
            UnityTasks.Unity(unitySettings => unitySettings
                .SetQuit(true)
                .SetBatchMode(true)
                .SetBuildTarget(UnityBuildTarget.StandaloneWindows64)
                .SetBuildWindows64Player(buildDirectory)
                .SetProjectPath(projectDirectory)
                .SetLogFile(RootDirectory / "NukeBuild" / "unity.log")
            );
        });
}
