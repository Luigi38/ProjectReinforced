using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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

using ProjectReinforced.Clients;
using ProjectReinforced.Types;
using ProjectReinforced.Others;
using ProjectReinforced.Properties;

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

        private static VideoWriter _videoWriter;
        private static readonly Queue<Mat> _screenshots = new Queue<Mat>();
        private static bool _isWorking = false;

        public static bool IsRecording { get; private set; }
        public static bool IsDisposed { get; private set; }

        public static async Task Record(IGameClient game)
        {
            Rectangle rect = new Rectangle();

            if (Win32.GetWindowRect(game.GameProcess.MainWindowHandle, ref rect))
            {
                await Task.Run(() => Start(game, rect));
            }
            else
            {
                ExceptionManager.ShowError("현재 플레이 중인 게임 윈도우를 찾지 못했습니다.", "게임 윈도우를 찾지 못함", nameof(Screen),
                    nameof(Record));
            }
        }

        private static void Start(IGameClient game, Rectangle rect)
        {
            int resolution = Settings.Default.Screen_Resolution;
            int seconds = Settings.Default.Screen_RecordTimeBeforeSave;
            double fps = Settings.Default.Screen_Fps;

            int maxSize = (int)fps * seconds; //큐의 최대 크기
            int delay = (int)fps / 1000;

            IsRecording = true;

            while (IsRecording)
            {
                if (!game.IsRunning || !game.IsActive) //게임을 껐거나 현재 활성화가 되지 않은 경우
                {
                    IsRecording = false;
                    ClearScreenshots();

                    break;
                }

                if (_screenshots.Count > maxSize)
                {
                    Mat mat = _screenshots.Dequeue();
                    mat.Dispose();
                }

                var bitmap = TakeScreenShot(rect);
                Mat frame = bitmap.ToMat();

                //해상도 조절
                if (resolution != 0)
                {
                    Size size = new Size(1920, 1080);

                    if (resolution == 720) size = new Size(1280, 720); //720p (HD)
                    frame.Resize(size);
                }

                _screenshots.Enqueue(frame);
                Thread.Sleep(delay);
            }
        }

        public static Highlight Stop(HighlightInfo info)
        {
            if (_screenshots.Count <= 0) return null;
            if (!IsRecording) return null;

            IsRecording = false;
            _isWorking = true;

            Highlight highlight = new Highlight(info);
            FourCC fcc = FourCC.FromString(Settings.Default.Screen_FourCC);

            if (File.Exists(highlight.VideoPath)) File.Delete(highlight.VideoPath);

            Mat baseScreen = _screenshots.Peek();
            Size size = baseScreen.Size();

            string path = Highlight.GetVideoPath(info);
            string directoryPath = Path.GetDirectoryName(path);

            if (File.Exists(path)) File.Delete(path);
            if (directoryPath != null && !Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            _videoWriter = new VideoWriter(path, fcc, Settings.Default.Screen_Fps, size);

            if (!_videoWriter.IsOpened())
            {
                _videoWriter.Release();
                baseScreen.Dispose();

                ExceptionManager.ShowError("하이라이트 영상을 저장할 수 없습니다.", "저장할 수 없음", nameof(Screen), nameof(Stop));
            }

            while (_screenshots.Count > 0)
            {
                Mat mat = _screenshots.Dequeue();

                if (mat.Size() != size)
                {
                    mat.Resize(size);
                }

                _videoWriter.Write(mat);
            }

            _videoWriter.Release();
            baseScreen.Dispose();
            _isWorking = true;

            return highlight;
        }

        public static Bitmap TakeScreenShot(Rectangle rect)
        {
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            var bmpScreenshot = new Bitmap(width, height,
                                   PixelFormat.Format32bppArgb);

            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            gfxScreenshot.CopyFromScreen(rect.left,
                                        rect.top,
                                        0,
                                        0,
                                        new System.Drawing.Size(width, height),
                                        CopyPixelOperation.SourceCopy);

            return new Bitmap(bmpScreenshot);
        }

        public static async Task WorkForRecordingAsync()
        {
            while (!IsDisposed)
            {
                if (!IsRecording && !_isWorking)
                {
                    IGameClient activeClient = ClientManager.CurrentClient;
                    await Record(activeClient);
                }

                await Task.Delay(1000);
            }
        }

        public static void Dispose()
        {
            IsDisposed = true;

            ClearScreenshots();
        }

        /// <summary>
        /// 메모리에 있던 스크린샷들을 제거합니다.
        /// </summary>
        private static void ClearScreenshots()
        {
            while (_screenshots.Count > 0)
            {
                Mat mat = _screenshots.Dequeue();
                mat.Dispose();
            }
        }
    }
}
