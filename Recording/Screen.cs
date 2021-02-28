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

using DesktopDuplication;

using ProjectReinforced.Clients;
using ProjectReinforced.Types;
using ProjectReinforced.Others;
using ProjectReinforced.Extensions;
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
        private static Queue<Mat> _screenshots = new Queue<Mat>();

        private static DesktopDuplicator _dd = new DesktopDuplicator(0);

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
            int seconds = Settings.Default.Screen_RecordTimeBeforeHighlight;
            int fps = Settings.Default.Screen_Fps;

            bool isUnfixed = fps == 0;
            //큐의 최대 크기 (고정되지 않은 프레임 방식이면 무한)
            int maxSize = !isUnfixed ? fps * seconds : -1;
            int frameDelay = !isUnfixed ? 1000 / fps : 0;

            Stopwatch sw = new Stopwatch();
            IsRecording = true;

            while (IsRecording)
            {
                sw.Start();

                if (game.IsRunning && game.IsActive) //게임을 키고 현재 플레이 중인 경우
                {
                    if (!isUnfixed && _screenshots.Count > maxSize)
                    {
                        Mat mat = _screenshots.Dequeue();
                        mat.Dispose();
                    }

                    if (!isUnfixed && _screenshots.Count > maxSize)
                    {
                        Mat mat = _screenshots.Dequeue();
                        mat.Dispose();
                    }

                    Mat frame = Screenshot(rect)?.ToMat();

                    if (frame != null)
                    {
                        //해상도 조절
                        if (resolution != 0)
                        {
                            Size size = new Size(1920, 1080);

                            if (resolution == 720) size = new Size(1280, 720); //720p (HD)
                            frame.Resize(size);
                        }

                        _screenshots.Enqueue(frame);
                        sw.Stop();

                        //원래 쉬어야 하는 delay 보다 이미 지나간 시간을 빼줘야 함. 안그러면 delay 시간 만큼 더 쉬게 됨.
                        int delay = frameDelay - (int)sw.ElapsedMilliseconds;
                        if (delay > 0) Thread.Sleep(delay);

                        sw.Reset();
                    }
                }
                else if (!game.IsRunning)
                {
                    IsRecording = false;
                    ClearScreenshots();

                    break;
                }
                else if (!game.IsActive)
                {
                    if (_screenshots.Count > 0) ClearScreenshots();
                    Thread.Sleep(30);
                }
            }

            IsRecording = false;
        }

        public static Highlight Stop(HighlightInfo info)
        {
            if (_screenshots.Count <= 0 || !IsRecording) return null;

            int secondsAfterHighlight = Settings.Default.Screen_RecordTimeAfterHighlight;
            bool isUnfixed = Settings.Default.Screen_Fps == 0;

            if (secondsAfterHighlight > 0) Thread.Sleep(secondsAfterHighlight * 1000);

            IsRecording = false;
            _isWorking = true;

            Highlight highlight = new Highlight(info);
            FourCC fcc = FourCC.FromString(Settings.Default.Screen_FourCC);

            if (File.Exists(highlight.VideoPath)) File.Delete(highlight.VideoPath);

            Mat baseScreen = _screenshots.Peek();
            Size size = baseScreen.Size();
            double fps = Settings.Default.Screen_Fps;

            string path = Highlight.GetVideoPath(info);
            string directoryPath = Path.GetDirectoryName(path);

            //고정되지 않은 프레임 방식 (저장된 프레임의 수에 따라서 fps가 결정됨)
            if (isUnfixed) fps = (double)_screenshots.Count / (Settings.Default.Screen_RecordTimeBeforeHighlight + secondsAfterHighlight);

            if (File.Exists(path)) File.Delete(path);
            if (directoryPath != null && !Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            _videoWriter = new VideoWriter(path, fcc, fps, size);

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
        #region Debugging Functions
        #region Record Function
        public static void RecordForDebug(double fps, int seconds)
        {
            Rectangle rect = new Rectangle //1920x1080 해상도
            {
                left = 0,
                top = 0,
                right = 1920,
                bottom = 1080
            };

            _ = Task.Run(() => StartForDebug(rect, fps, seconds));
        }
        #endregion
        #region Start Function
        private static void StartForDebug(Rectangle rect, double fps, int seconds)
        {
            bool isUnfixed = fps.EqualsPrecision(0.0);
            //큐의 최대 크기 (고정되지 않은 프레임 방식이면 무한)
            int maxSize = !isUnfixed ? (int)fps * seconds : -1;
            int frameDelay = !isUnfixed ? 1000 / (int) fps : 0;

            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            Stopwatch sw = new Stopwatch();
            IsRecording = true;
            
            while (IsRecording)
            {
                sw.Start();

                if (!isUnfixed && _screenshots.Count > maxSize)
                {
                    Mat mat = _screenshots.Dequeue();
                    mat.Dispose();
                }

                Mat frame = Screenshot(rect)?.ToMat();

                if (frame != null)
                {
                    _screenshots.Enqueue(frame);
                }

                sw.Stop();

                //원래 쉬어야 하는 delay 보다 이미 지나간 시간을 빼줘야 함. 안그러면 delay 시간 만큼 더 쉬게 됨.
                int delay = frameDelay - (int) sw.ElapsedMilliseconds;
                sw.Start();
                if (delay > 0) Thread.Sleep(delay);

                Trace.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}");
                sw.Reset();
            }
        }
        #endregion
        #region Stop Function
        public static bool StopForDebug(GameType game, double fps, int secondsBeforeHighlight, int secondsAfterHighlight, out int frameCount)
        {
            if (_screenshots.Count <= 0 || !IsRecording)
            {
                frameCount = 0;
                return false;
            }
;
            bool isUnfixed = fps.EqualsPrecision(0.0);
            if (secondsAfterHighlight > 0) Thread.Sleep(secondsAfterHighlight * 1000);

            IsRecording = false;
            _isWorking = true;
            frameCount = _screenshots.Count;

            Mat baseScreen = _screenshots.Peek();
            Size size = baseScreen.Size();

            string path = Highlight.GetVideoPath(game, $"{Highlight.GetHighlightFileName(DateTime.Now)}.mp4");
            string directoryPath = Path.GetDirectoryName(path);

            //고정되지 않은 프레임 방식 (저장된 프레임의 수에 따라서 fps가 결정됨)
            if (isUnfixed) fps = (double) _screenshots.Count / (secondsBeforeHighlight + secondsAfterHighlight);

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
        #endregion
        #region Record Test Function
        public static async Task RecordTestForDebugAsync(int waitMilliseconds, double fps, int secondsBeforeHighlight, int secondsAfterHighlight)
        {
            RecordForDebug(fps, secondsBeforeHighlight);
            await Task.Delay(waitMilliseconds);

            bool recorded = StopForDebug(GameType.R6, fps, secondsBeforeHighlight, secondsAfterHighlight, out var frameCount);
            MessageBox.Show($"OK : {recorded}, Count : {frameCount}");
        }
        #endregion
        #endregion

        public static Bitmap Screenshot(Rectangle rect)
        {
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            //https://stackoverflow.com/questions/52930179/fully-real-time-screen-capture-in-window-8-10-technology-without-delay/52935517
            ////https://stackoverflow.com/questions/6812068/c-sharp-which-is-the-fastest-way-to-take-a-screen-shot

            //C++ Wrapper
            //https://luckygg.tistory.com/221

            //DirectX / Direct3D
            //https://github.com/spazzarama/Direct3DHook
            //https://spazzarama.com/2011/03/14/c-screen-capture-and-overlays-for-direct3d-9-10-and-11-using-api-hooks/

            //SlimDX
            //https://gamedev.net/forums/topic/662374-slimdx-to-take-fullscreen-game-screenshots/5190656/

            DesktopFrame frame = _dd.GetLatestFrame();
            Bitmap frameBitmap = frame?.DesktopImage;

            //비트맵을 특정한 영역으로 자르기
            if (frameBitmap != null && frameBitmap.Width == width && frameBitmap.Height == height) return frameBitmap;
            return frameBitmap?.Clone(new System.Drawing.Rectangle(rect.left, rect.top, width, height), frameBitmap.PixelFormat);
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
            _screenshots = null;
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
