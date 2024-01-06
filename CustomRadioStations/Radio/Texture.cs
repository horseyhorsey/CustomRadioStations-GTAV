using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security;
using System.Runtime.InteropServices;

namespace SelectorWheel
{
    public class Texture
    {
        private static readonly Dictionary<string, int> _textures = new Dictionary<string, int>();
        public Texture(string path, int index)
        {
            Path = path;
            Index = index;
        }

        public int DrawLevel { get; }
        public int Index { get; }
        public string Path { get; }

        /// <summary>
        /// Creates a texture. Texture deletion is performed automatically when game reloads scripts.
        /// Can be called only in the same thread as natives.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>Internal texture ID.</returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport("ScriptHookV.dll", ExactSpelling = true, EntryPoint = "?createTexture@@YAHPEBD@Z")]
        public static extern int CreateTexture([MarshalAs(UnmanagedType.LPStr)] string filename);

        public static void DrawTexture(string filename, int index, int level, int time, Point pos, Size size)
        {
            DrawTexture(filename, index, level, time, pos, new PointF(0.0f, 0.0f), size, 0.0f, Color.White, 1.0f);
        }

        public static void DrawTexture(string filename, int index, int level, int time, Point pos, PointF center, Size size, float rotation, Color color, float aspectRatio)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(filename);
            }

            int id;

            if (_textures.TryGetValue(filename, out int texture))
            {
                id = texture;
            }
            else
            {
                id = CreateTexture(filename);

                _textures.Add(filename, id);
            }

            float x = (float)pos.X / GTA.UI.Screen.Width;
            float y = (float)pos.Y / GTA.UI.Screen.Height;
            float w = (float)size.Width / GTA.UI.Screen.Width;
            float h = (float)size.Height / GTA.UI.Screen.Height;

            DrawTexture(id, index, level, time, w, h, center.X, center.Y, x, y, rotation, aspectRatio, color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }

        /// <summary>
        /// Draws a texture on screen. Can be called only in the same thread as natives.
        /// </summary>
        /// <param name="id">Texture ID returned by <see cref="CreateTexture(string)"/>.</param>
        /// <param name="instance">The instance index. Each texture can have up to 64 different instances on screen at a time.</param>
        /// <param name="level">Texture instance with low levels draw first.</param>
        /// <param name="time">How long in milliseconds the texture instance should stay on screen.</param>
        /// <param name="sizeX">Width in screen space [0,1].</param>
        /// <param name="sizeY">Height in screen space [0,1].</param>
        /// <param name="centerX">Center position in texture space [0,1].</param>
        /// <param name="centerY">Center position in texture space [0,1].</param>
        /// <param name="posX">Position in screen space [0,1].</param>
        /// <param name="posY">Position in screen space [0,1].</param>
        /// <param name="rotation">Normalized rotation [0,1].</param>
        /// <param name="scaleFactor">Screen aspect ratio, used for size correction.</param>
        /// <param name="colorR">Red tint.</param>
        /// <param name="colorG">Green tint.</param>
        /// <param name="colorB">Blue tint.</param>
        /// <param name="colorA">Alpha value.</param>
        [SuppressUnmanagedCodeSecurity]
        [DllImport("ScriptHookV.dll", ExactSpelling = true, EntryPoint = "?drawTexture@@YAXHHHHMMMMMMMMMMMM@Z")]
        public static extern void DrawTexture(int id, int instance, int level, int time, float sizeX, float sizeY, float centerX, float centerY, float posX, float posY, float rotation, float scaleFactor, float colorR, float colorG, float colorB, float colorA);

        public void Draw(int level, int time, Point pos, PointF center, Size size, float rotation, Color color, float aspectRatio)
        {
            DrawTexture(Path, Index, level, time, pos, center, size, rotation, color, aspectRatio);
        }

        /// <summary> Draws texture</summary>
        public void StopDraw() => DrawTexture(Path, Index, 1, 0, new Point(1280, 720), new Size(0, 0));
    }
}