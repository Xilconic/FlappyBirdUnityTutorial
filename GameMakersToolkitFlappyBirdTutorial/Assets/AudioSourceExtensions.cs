using UnityEngine;

namespace Assets
{
    internal static class AudioSourceExtensions
    {
        /// <summary>
        /// Cause <paramref name="source"/> to play <paramref name="clip"/>.
        /// </summary>
        public static void PlayClip(this AudioSource source, AudioClip clip)
        {
            Debug.Assert(source != null, "'source' cannot be null!");
            Debug.Assert(clip != null, "''clip' cannot be null!");

            source.clip = clip;
            source.Play();
        }
    }
}
