using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

using OpenCvSharp;
using ProjectReinforced.Types;

namespace ProjectReinforced.Recording
{
    public class Screen
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Rectangle
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle Rect);

        private static VideoWriter _videoWriter;
        private static Dictionary<GameType, Highlight> _highlightMap = new Dictionary<GameType, Highlight>();

        public static void Record(GameType gameType, Process game)
        {
            Rectangle rect = new Rectangle();

            if (GetWindowRect(game.MainWindowHandle, ref rect))
            {
                string path = Highlight.GetVideoPath(gameType, "temp");
                FourCC fourCC = FourCC.H265;
                double fps = 30.0;
                Size size = new Size(rect.right - rect.left, rect.bottom - rect.top);

                _videoWriter = new VideoWriter(path, fourCC, fps, size);
                Task.Run(() => Start());
            }
        }

        private static void Start()
        {
            //https://m.blog.naver.com/hoan123432/221923826254 : VideoWriter
            //https://stackoverflow.com/questions/51260404/continuous-capture-desktop-loop-with-opencv : Bitmap To Mat
        }

        public static Highlight Stop(GameType gameType, int seconds)
        {
            Highlight highlight = null;



            _highlightMap.TryGetValue(gameType, out highlight);
            return highlight;
        }

        public static System.Drawing.Bitmap TakeScreenshot(Rectangle rect)
        {
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            var bmpScreenshot = new System.Drawing.Bitmap(width, height,
                                   System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var gfxScreenshot = System.Drawing.Graphics.FromImage(bmpScreenshot);

            gfxScreenshot.CopyFromScreen(rect.left,
                                        rect.top,
                                        0,
                                        0,
                                        new System.Drawing.Size(width, height),
                                        System.Drawing.CopyPixelOperation.SourceCopy);

            return new System.Drawing.Bitmap(bmpScreenshot);
        }
    }
}
