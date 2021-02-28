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

        private static readonly ScreenStateLogger _screenStateLogger = new ScreenStateLogger();
        private static readonly Queue<Mat> _screenshots = new Queue<Mat>();

        private static bool _isWorking;

        public static bool IsRecording { get; private set; }
        public static bool IsDisposed { get; private set; }

        public static async Task Record(IGameClient game)
        {
            if (game == null) return;

            Rectangle rect = new Rectangle();
            rect.left = 0;
            rect.right = 1920;
            rect.top = 0;
            rect.bottom = 1080;

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
                if (game.IsRunning && game.IsActive) //게임을 키고 현재 플레이 중인 경우
                {
                    if (_screenshots.Count > maxSize)
                    {
                        Mat mat = _screenshots.Dequeue();
                        mat.Dispose();
                    }

                    //var bitmap = Screenshot(rect);
                    Mat frame = null;

                    //해상도 조절
                    if (resolution != 0)
                    {
                        Size size = new Size(1920, 1080);

                        if (resolution == 720) size = new Size(1280, 720); //720p (HD)
                        frame.Resize(size);
                    }

                    _screenshots.Enqueue(frame);
                }
                else if (!game.IsRunning)
                {
                    IsRecording = false;
                    ClearScreenshots();

                    break;
                }

                Cv2.WaitKey(delay);
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
                return null;
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
            _isWorking = false;

            return highlight;
        }

        public static void RecordForDebug()
        {
            Rectangle rect = new Rectangle //1920x1080 해상도
            {
                left = 0,
                top = 0,
                right = 1920,
                bottom = 1080
            };

            _ = Task.Run(() => StartForDebug(rect));
        }

        private static void StartForDebug(Rectangle rect)
        {
            int seconds = 15; //하이라이트가 나오기 전의 15초 전을 저장
            double fps = 30.0; //30프레임

            int maxSize = (int)fps * seconds; //큐의 최대 크기
            int delay = (int)fps / 1000;

            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            Stopwatch sw = new Stopwatch();
            
            _screenStateLogger.ScreenRefreshed += (object s, Bitmap data) =>
            {
                sw.Stop();
                Trace.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}");

                if (_screenshots.Count > maxSize)
                {
                    Mat mat = _screenshots.Dequeue();
                    mat.Dispose();
                }

                Mat frame = data.ToMat();
                Cv2.ImShow("TEST", frame);
                Cv2.WaitKey();

                _screenshots.Enqueue(frame);

                sw.Reset();
                sw.Start();
            };

            IsRecording = true;
            _screenStateLogger.Start();
        }

        public static bool StopForDebug(GameType game)
        {
            if (_screenshots.Count <= 0) return false;
            if (!IsRecording) return false;

            IsRecording = false;
            _screenStateLogger.Stop();

            _isWorking = true;

            Mat baseScreen = _screenshots.Peek();
            Size size = baseScreen.Size();

            string path = Highlight.GetVideoPath(game, $"{Highlight.GetHighlightFileName(DateTime.Now)}.mp4");
            string directoryPath = Path.GetDirectoryName(path);

            double fps = 30.0; //30프레임

            if (File.Exists(path)) File.Delete(path);
            if (directoryPath != null && !Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            _videoWriter = new VideoWriter(path, FourCC.H265, fps, size);

            if (!_videoWriter.IsOpened())
            {
                _videoWriter.Release();
                baseScreen.Dispose();

                ExceptionManager.ShowError("하이라이트 영상을 저장할 수 없습니다.", "저장할 수 없음", nameof(Screen), nameof(Stop));
                return false;
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
            _isWorking = false;

            return true;
        }

        public static Bitmap Screenshot(Rectangle rect)
        {
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            //https://stackoverflow.com/questions/52930179/fully-real-time-screen-capture-in-window-8-10-technology-without-delay/52935517
            //https://luckygg.tistory.com/221
            //https://stackoverflow.com/questions/6812068/c-sharp-which-is-the-fastest-way-to-take-a-screen-shot

            return null;
        }

        public static Bitmap Screenshot(int x, int y, int width, int height)
        {
            Rectangle rect = new Rectangle
            {
                left = x,
                top = y,
                right = x + width,
                bottom = y + height
            };
            return Screenshot(rect);
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
