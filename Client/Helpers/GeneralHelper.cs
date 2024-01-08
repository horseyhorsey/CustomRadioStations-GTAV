using System;
using System.Drawing;

namespace CustomRadioStations
{
    static class GeneralHelper
    {
        /// <summary> windows IWshRuntimeLibrary NOT USED this version </summary>
        /// <param name="shortcutFilePath"></param>
        /// <returns></returns>
        //public static string GetShortcutTargetFile(string shortcutFilePath)
        //{
        //    IWshRuntimeLibrary.IWshShell wsh = new IWshRuntimeLibrary.WshShell();
        //    IWshRuntimeLibrary.IWshShortcut sc = (IWshRuntimeLibrary.IWshShortcut)wsh.CreateShortcut(shortcutFilePath);

        //    if (System.IO.File.Exists(sc.TargetPath))
        //        return sc.TargetPath;
        //    else
        //        return string.Empty;
        //}

        /// <summary> limit range </summary>
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

    public static class ColorTranslator
    {
        public static Color FromHtml(string hex)
        {
            hex = hex.TrimStart('#');

            if (hex.Length == 6)
            {
                // For RGB format (e.g., #RRGGBB)
                return Color.FromArgb(
                    int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)
                );
            }
            else if (hex.Length == 8)
            {
                // For ARGB format (e.g., #AARRGGBB)
                return Color.FromArgb(
                    int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber)
                );
            }
            else
            {
                throw new ArgumentException("Invalid hex color code length.");
            }
        }
    }

}
