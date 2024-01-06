using System.Drawing;

namespace CustomRadioStations
{
    static class GeneralHelper
    {
        /// <summary> windows IWshRuntimeLibrary  </summary>
        /// <param name="shortcutFilePath"></param>
        /// <returns></returns>
        public static string GetShortcutTargetFile(string shortcutFilePath)
        {
            IWshRuntimeLibrary.IWshShell wsh = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut sc = (IWshRuntimeLibrary.IWshShortcut)wsh.CreateShortcut(shortcutFilePath);

            if (System.IO.File.Exists(sc.TargetPath))
                return sc.TargetPath;
            else
                return string.Empty;
        }

        public static float LimitToRange(
        this float value, float inclusiveMinimum, float inclusiveMaximum)
        {
            if (value < inclusiveMinimum) { return inclusiveMinimum; }
            if (value > inclusiveMaximum) { return inclusiveMaximum; }
            return value;
        }

        public static string ColorToHex(Color color)
        {
            return "#" + color.ToArgb().ToString("X");
        }

        public static Color HexToColor(string hex)
        {
            return ColorTranslator.FromHtml(hex);
        }
    }
}
