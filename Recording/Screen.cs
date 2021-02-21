using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Windows;
using OpenCvSharp;
using OpenCvSharp.Extensions;

using ProjectReinforced.Types;
using Size = OpenCvSharp.Size;

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
        static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle rect);

        private static VideoWriter _videoWriter;
        private static bool _isStopped = true;
        private static readonly Queue<Mat> _screenShots = new Queue<Mat>();

        public static void Record(GameType gameType, Process game)
        {
            Rectangle rect = new Rectangle();

            if (GetWindowRect(game.MainWindowHandle, ref rect))
            {
                int beforeSeconds = 30;
                double fps = 30.0;

                _isStopped = false;
                Task.Run(() => Start(rect, beforeSeconds, fps));
            }
        }

        private static void Start(Rectangle rect, int seconds, double fps)
        {
            int maxSize = (int)fps * seconds; //큐의 최대 크기
            int delay = (int)fps / 1000;

            while (!_isStopped)
            {
                if (_screenShots.Count > maxSize)
                {
                    Mat mat = _screenShots.Dequeue();
                    mat.Dispose();
                }

                var bitmap = TakeScreenShot(rect);
                Mat frame = bitmap.ToMat();

                _screenShots.Enqueue(frame);
                Thread.Sleep(delay);
            }
        }

        public static Highlight Stop(HighlightInfo info)
        {
            if (_screenShots.Count <= 0) return null;

            _isStopped = true;
            Highlight highlight = new Highlight(info);

            if (File.Exists(highlight.VideoPath)) File.Delete(highlight.VideoPath);

            FourCC fourCC = FourCC.H265;
            double fps = 30.0;

            Mat baseScreen = _screenShots.Peek();
            Size size = baseScreen.Size();

            string path = Highlight.GetVideoPath(info);
            string directoryPath = Path.GetDirectoryName(path);

            if (File.Exists(path)) File.Delete(path);
            if (directoryPath != null && !Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            _videoWriter = new VideoWriter(path, fourCC, fps, size);

            if (!_videoWriter.IsOpened())
            {
                _videoWriter.Release();
                baseScreen.Dispose();

                throw new IOException("Can't save highlight video.");
            }

            while (_screenShots.Count > 0)
            {
                Mat mat = _screenShots.Dequeue();

                _videoWriter.Write(mat);
            }

            _videoWriter.Release();
            baseScreen.Dispose();

            return highlight;
        }

        public static System.Drawing.Bitmap TakeScreenShot(Rectangle rect)
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
