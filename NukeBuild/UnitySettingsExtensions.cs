using Nuke.Common.IO;
using Nuke.Common.Tools.Unity;

namespace _build
{
    public static class UnitySettingsExtensions
    {
        public static UnitySettings SetWindows32BitBuildTarget(
            this UnitySettings settings,
            AbsolutePath targetAppFileLocation) => settings
                .SetBuildTarget(UnityBuildTarget.StandaloneWindows)
                .SetBuildWindowsPlayer(targetAppFileLocation);

        public static UnitySettings SetWindows64BitBuildTarget(
            this UnitySettings settings,
            AbsolutePath targetAppFileLocation) => settings
                .SetBuildTarget(UnityBuildTarget.StandaloneWindows64)
                .SetBuildWindows64Player(targetAppFileLocation);
    }
}
