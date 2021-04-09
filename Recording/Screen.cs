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

using NAudio;
using NAudio.Wave;
using NAudio.Lame;

using ProjectReinforced.Clients;
using ProjectReinforced.Clients.Types;
using ProjectReinforced.Recording.Types;
using ProjectReinforced.Extensions;
using ProjectReinforced.Others;
using ProjectReinforced.Properties;

using Size = OpenCvSharp.Size;

namespace ProjectReinforced.Recording
{
    public class Screen
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public int Width => right - left;
            public int Height => bottom - top;

            public static explicit operator Rectangle(RECT rect)
            {
                return new Rectangle(rect.left, rect.top, rect.Width, rect.Height);
            }

            public static explicit operator Size(RECT rect)
            {
                return new Size(rect.Width, rect.Height);
            }
        }

        private static VideoWriter _videoWriter;
        /// <summary>
        /// 화면 녹화용
        /// </summary>
        private static DesktopDuplicator _dd = new DesktopDuplicator(0);
        private static Queue<ScreenCaptured> _frames = new Queue<ScreenCaptured>(3600); //60fps * 60seconds

        public static string FFmpegExecutablePath { get; set; } = string.Empty;

        private static bool _isWorking;

        public static bool IsRecording { get; private set; }
        public static bool IsDisposed { get; private set; }

        public static async Task Record(IGameClient game)
        {
            if (game == null) return;

            RECT rect = new RECT
            {
                left = 0,
                right = 1920,
                top = 0,
                bottom = 1080
            };

            if (Win32.GetWindowRect(game.GameProcess.MainWindowHandle, ref rect))
            {
                await Task.Run(() => Start(game, (Rectangle)rect));
            }
            else
            {
                ExceptionManager.ShowError("현재 플레이 중인 게임 윈도우를 찾지 못했습니다.", "게임 윈도우를 찾지 못함", nameof(Screen),
                    nameof(Record));
            }
        }

        private static void Start(IGameClient game, Rectangle bounds)
        {
            int resolution = Settings.Default.Screen_Resolution;
            int seconds = Settings.Default.Screen_RecordTimeBeforeHighlight;
            int fps = Settings.Default.Screen_Fps;

            bool isUnfixed = fps == 0;
            //큐의 최대 크기 (고정되지 않은 프레임 방식이면 무한)
            int maxSize = !isUnfixed ? fps * seconds : -1;
            int frameDelay = !isUnfixed ? 1000 / fps : 0;

            ScreenCaptured lastScreen = null;
            Stopwatch sw = new Stopwatch();
            IsRecording = true;

            //소리 녹음
            Audio.Record();

            //스크린샷 시 실행 되는 코드
            while (IsRecording)
            {
                if (game.IsRunning && game.IsActive) //게임을 키고 현재 플레이 중인 경우
                {
                    sw.Start();

                    if (!isUnfixed && _frames.Count > maxSize)
                    {
                        ScreenCaptured sc = _frames.Peek();

                        if (sc.CountToUse == 0) //CountToUse 값 할당 (Elapsed 기준)
                        {
                            sc.CountToUse = sc.GetCountToUseByElapsed(fps);
                        }

                        if (--sc.CountToUse == 0)
                        {
                            sc.Frame.Dispose();
                            _frames.Dequeue();
                        }
                    }

                    //프레임 가져오기
                    Mat frame = Screenshot(bounds)?.ToMat();

                    if (frame != null)
                    {
                        Size size = frame.Size();

                        //해상도 조절
                        if (resolution != 0)
                        {
                            switch (resolution)
                            {
                                case 1080:
                                    size = new Size(1920, 1080);
                                    break;
                                case 720:
                                    size = new Size(1280, 720); //720p (HD)
                                    break;
                            }
                        }

                        lastScreen = new ScreenCaptured(frame, size, sw.ElapsedMilliseconds);
                        _frames.Enqueue(lastScreen);

                        sw.Stop();
                        frame.Dispose();
                    }
                    else
                    {
                        if (lastScreen != null)
                        {
                            sw.Stop();
                            lastScreen.ElapsedMilliseconds += sw.ElapsedMilliseconds;

                            sw.Reset();
                        }
                    }
                }
                else if (!game.IsRunning)
                {
                    IsRecording = false;
                    ClearFrames();
                }
                else if (!game.IsActive)
                {
                    ClearFrames();
                    Thread.Sleep(30);
                }
            }
        }

        private static async Task<Highlight> Stop(HighlightInfo info)
        {
            if (_frames.Count <= 0 || !IsRecording) return null;

            int secondsAfterHighlight = Settings.Default.Screen_RecordTimeAfterHighlight;
            double fps = Settings.Default.Screen_Fps;

            bool isUnfixed = (int)fps == 0;
            int frameDelay = !isUnfixed ? 1000 / (int)fps : 0;

            if (secondsAfterHighlight > 0) Thread.Sleep(secondsAfterHighlight * 1000);

            IsRecording = false;
            _isWorking = true;

            string audioPath = await Audio.StopAsync();
            bool audioAvailable = !string.IsNullOrWhiteSpace(audioPath);

            Highlight highlight = new Highlight(info);
            FourCC fcc = FourCC.FromString(Settings.Default.Screen_FourCC);

            if (File.Exists(highlight.VideoPath)) File.Delete(highlight.VideoPath);

            Size size = _frames.Peek().FrameSize;

            string outputPath = Highlight.GetVideoPath(info);
            string path = audioAvailable ? MainWindow.GetTempFileName("mp4") : outputPath;
            string directoryPath = Path.GetDirectoryName(path);

            //고정되지 않은 프레임 방식 (저장된 프레임의 수에 따라서 fps가 결정됨)
            if (isUnfixed) fps = (double)_frames.Count / (Settings.Default.Screen_RecordTimeBeforeHighlight + secondsAfterHighlight);

            if (File.Exists(path)) File.Delete(path);
            if (directoryPath != null && !Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            _videoWriter = new VideoWriter(path, fcc, fps, size);

            if (!_videoWriter.IsOpened())
            {
                _videoWriter.Release();

                ExceptionManager.ShowError("하이라이트 영상을 저장할 수 없습니다.", "저장할 수 없음", nameof(Screen), nameof(Stop));
                return null;
            }

            ScreenCaptured lastScreen = null;

            while (_frames.Count > 0)
            {
                if (lastScreen == null || lastScreen.CountToUse == 0)
                {
                    lastScreen?.Frame?.Dispose(); //lastScreen 변수가 null이면 이 함수는 실행되지 않음

                    lastScreen = _frames.Dequeue();
                    lastScreen.CountToUse = Math.Max((int)lastScreen.ElapsedMilliseconds / frameDelay, 1); //스크린샷을 하는 데 걸린 시간을 기준으로 재활용 값 설정
                }

                if (lastScreen.Frame.Size() != size)
                {
                    lastScreen.Frame.Resize(size);
                }

                _videoWriter.Write(lastScreen.Frame);
                lastScreen.CountToUse--;
            }

            _videoWriter.Release();

            //영상 및 소리 합병
            if (audioAvailable && !string.IsNullOrWhiteSpace(FFmpegExecutablePath))
            {
                await AddAudio(path, audioPath, outputPath);

                //합병됐으므로 영상 및 소리 파일 삭제
                File.Delete(path);
                File.Delete(audioPath);
            }
            _isWorking = false;

            return highlight;
        }

        public static async Task<Highlight> StopAsync(HighlightInfo info)
        {
            return await Task.Run(() => Stop(info));
        }
        #region Debugging Functions
        #region Record Function
        public static void RecordForDebug(double fps, int seconds)
        {
            Rectangle rect = new Rectangle(0, 0, 1920, 1080); //1920x1080 해상도
            Size size = new Size(1920, 1080);

            Task.Run(() => StartForDebug(rect, size, fps, seconds));
        }
        #endregion
        #region Start Function
        private static void StartForDebug(Rectangle bounds, Size size, double fps, int seconds)
        {
            bool isUnfixed = (int)fps == 0;
            //큐의 최대 크기 (고정되지 않은 프레임 방식이면 무한)
            int maxSize = !isUnfixed ? (int)fps * seconds : -1;

            ScreenCaptured lastScreen = null;

            Stopwatch sw = new Stopwatch();
            IsRecording = true;

            //소리 녹음
            Audio.Record();

            while (IsRecording)
            {
                sw.Start();

                if (!isUnfixed && _frames.Count > maxSize)
                {
                    ScreenCaptured sc = _frames.Peek();

                    if (sc.CountToUse == 0) //CountToUse 값 할당 (Elapsed 기준)
                    {
                        sc.CountToUse = sc.GetCountToUseByElapsed((int)fps);
                    }

                    if (--sc.CountToUse == 0)
                    {
                        sc.Frame.Dispose();
                        _frames.Dequeue();
                    }
                }

                //프레임 가져오기
                Mat frame = Screenshot(bounds)?.ToMat();

                if (frame != null)
                {
                    lastScreen = new ScreenCaptured(frame, size, sw.ElapsedMilliseconds);
                    _frames.Enqueue(lastScreen);

                    frame.Dispose();

                    sw.Stop();
                    Trace.WriteLine($"{sw.ElapsedMilliseconds}ms elapsed.");

                    //int delay = Math.Max(0, frameDelay - (int)sw.ElapsedMilliseconds);
                    sw.Reset();
                }
                else
                {
                    if (lastScreen != null)
                    {
                        sw.Stop();
                        lastScreen.ElapsedMilliseconds += sw.ElapsedMilliseconds;

                        sw.Reset();
                    }
                }
            }
        }
        #endregion
        #region Stop Function
        public static async Task<int> StopAsyncForDebug(GameType game, double fps, int secondsBeforeHighlight, int secondsAfterHighlight)
        {
            if (_frames.Count <= 0 || !IsRecording)
            {
                return 0;
            }

            bool isUnfixed = (int)fps == 0;
            if (secondsAfterHighlight > 0) Thread.Sleep(secondsAfterHighlight * 1000);

            IsRecording = false;
            _isWorking = true;

            int frameCount = _frames.Count;
            DateTime finishedTime = DateTime.Now;

            string audioPath = await Audio.StopAsync();
            bool audioAvailable = !string.IsNullOrWhiteSpace(audioPath);

            Size size = _frames.Peek().FrameSize;
            string outputPath = Highlight.GetFilePath(game, Highlight.GetHighlightFileName(finishedTime, "mp4"));

            string path = audioAvailable ? MainWindow.GetTempFileName("mp4") : outputPath;
            string directoryPath = Path.GetDirectoryName(path);

            //고정되지 않은 프레임 방식 (저장된 프레임의 수에 따라서 fps가 결정됨)
            if (isUnfixed) fps = (double) _frames.Count / (secondsBeforeHighlight + secondsAfterHighlight);

            if (File.Exists(path)) File.Delete(path);
            if (directoryPath != null && !Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            _videoWriter = new VideoWriter(path, FourCC.H265, fps, size);

            if (!_videoWriter.IsOpened())
            {
                _videoWriter.Release();

                ExceptionManager.ShowError("하이라이트 영상을 저장할 수 없습니다.", "저장할 수 없음", nameof(Screen), nameof(Stop));
                return 0;
            }

            ScreenCaptured lastScreen = null;

            while (_frames.Count > 0)
            {
                if (lastScreen == null || lastScreen.CountToUse == 0)
                {
                    lastScreen?.Frame?.Dispose(); //lastScreen 변수가 null이면 이 함수는 실행되지 않음

                    lastScreen = _frames.Dequeue();
                    lastScreen.CountToUse = lastScreen.GetCountToUseByElapsed((int)fps);
                }

                if (lastScreen.Frame.Size() != size)
                {
                    lastScreen.Frame.Resize(size);
                }

                _videoWriter.Write(lastScreen.Frame);
                lastScreen.CountToUse--;
            }

            _videoWriter.Release();

            //영상 및 소리 합병
            if (audioAvailable && !string.IsNullOrWhiteSpace(FFmpegExecutablePath))
            {
                await AddAudio(path, audioPath, outputPath);

                //합병됐으므로 영상 및 소리 파일 삭제
                File.Delete(path);
                File.Delete(audioPath);
            }
            _isWorking = false;

            return frameCount;
        }
        #endregion
        #region Record Test Function
        public static async Task RecordTestForDebugAsync(int waitMilliseconds, double fps, int secondsBeforeHighlight, int secondsAfterHighlight)
        {
            RecordForDebug(fps, secondsBeforeHighlight);
            await Task.Delay(waitMilliseconds);

            int frameCount = await Task.Run(() => StopAsyncForDebug(GameType.R6, fps, secondsBeforeHighlight, secondsAfterHighlight));
            MessageBox.Show($"OK : {frameCount > 0}, Count : {frameCount}");
        }
        #endregion
        #endregion

        public static Bitmap Screenshot()
        {
            return Screenshot(Rectangle.Empty);
        }

        public static Bitmap Screenshot(Rectangle bounds)
        {
            DesktopFrame desktopFrame = _dd.GetLatestFrame();
            return desktopFrame != null ? CropImage(desktopFrame.DesktopImage, bounds) : null;
        }

        public static Bitmap Screenshot(int x, int y, int width, int height)
        {
            return Screenshot(new Rectangle(x, y, width, height));
        }

        /// <summary>
        /// 비트맵을 특정한 영역으로 자릅니다.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        private static Bitmap CropImage(Bitmap image, Rectangle rect)
        {
            if (rect == Rectangle.Empty || (image != null && image.Width == rect.Width && image.Height == rect.Height)) return image;
            return image?.Clone(rect, image.PixelFormat);
        }

        /// <summary>
        /// 영상 파일에 소리를 추가 합니다. (https://stackoverflow.com/questions/53584389/combine-audio-and-video-files-with-ffmpeg-in-c-sharp)
        /// </summary>
        /// <param name="videoPath">영상 파일 경로</param>
        /// <param name="audioPath">소리 파일 경로</param>
        /// <param name="outPath">출력 파일 경로</param>
        /// <returns></returns>
        public static async Task AddAudio(string videoPath, string audioPath, string outPath)
        {
            string args = $"-i \"{videoPath}\" -i \"{audioPath}\" -preset ultrafast -tune fastdecode -shortest \"{outPath}\"";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "ffmpeg.exe",
                WorkingDirectory = FFmpegExecutablePath,
                Arguments = args
            };

            using (Process exeProcess = Process.Start(startInfo))
            {
                await exeProcess.WaitForExitAsync();
            }
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
            IsRecording = false;

            ClearFrames();
            _frames = null;
        }

        /// <summary>
        /// 메모리에 있던 프레임들을 제거합니다.
        /// </summary>
        private static void ClearFrames()
        {
            while (_frames.Count > 0)
            {
                ScreenCaptured sc = _frames.Dequeue();
                sc.Frame.Dispose();
            }
        }
    }
}
