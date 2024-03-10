using UnityEngine.UI;

namespace Assets
{
    internal static class TextExtensions
    {
        public static void UpdateText(this Text text, int number)
        {
            text.text = number.ToString();
        }
    }
}
