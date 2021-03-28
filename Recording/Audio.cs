using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Lame;

namespace ProjectReinforced.Recording
{
    public class Audio
    {
        /// <summary>
        /// 소리 녹음용 (스피커)
        /// </summary>
        private static WasapiLoopbackCapture _capture;
        /// <summary>
        /// 소리 녹음용 (마이크)
        /// </summary>
        private static WasapiCapture _captureMic;

        private static Queue<WaveInEventArgs> _sounds = new Queue<WaveInEventArgs>();
        private static Queue<WaveInEventArgs> _soundsMic = new Queue<WaveInEventArgs>();

        /// <summary>
        /// 기본 마이크 장치
        /// </summary>
        private static MMDevice DefaultMMDeviceIn
        {
            get
            {
                try
                {
                    return WasapiCapture.GetDefaultCaptureDevice();
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 기본 스피커 장치
        /// </summary>
        private static MMDevice DefaultMMDeviceOut
        {
            get
            {
                try
                {
                    return WasapiLoopbackCapture.GetDefaultLoopbackCaptureDevice();
                }
                catch
                {
                    return null;
                }
            }
        }

        private static string _prevInDeviceId = string.Empty;
        private static string _prevOutDeviceId = string.Empty;

        /// <summary>
        /// 초기화가 되어 있는가?
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Audio 클래스를 초기화합니다.
        /// </summary>
        public static void Initialize()
        {
            InitializeCaptureIn();
            InitializeCaptureOut();

            IsInitialized = true;
        }

        /// <summary>
        /// 입력 캡처 장치를 초기화 합니다.
        /// </summary>
        private static void InitializeCaptureIn()
        {
            if (DefaultMMDeviceIn != null)
            {
                _captureMic = new WasapiCapture(DefaultMMDeviceIn);
                _prevInDeviceId = DefaultMMDeviceIn.ID;
            }
        }

        /// <summary>
        /// 출력 캡처 장치를 초기화 합니다.
        /// </summary>
        private static void InitializeCaptureOut()
        {
            if (DefaultMMDeviceOut != null)
            {
                _capture = new WasapiLoopbackCapture(DefaultMMDeviceOut);
                _prevOutDeviceId = DefaultMMDeviceOut.ID;
            }
        }

        /// <summary>
        /// 컴퓨터 소리를 녹음합니다.
        /// </summary>
        /// <returns>정상적으로 녹음이 되고 있는지의 여부</returns>
        public static bool Record()
        {
            return Record(DefaultMMDeviceIn != null);
        }

        /// <summary>
        /// 컴퓨터 소리를 녹음합니다.
        /// </summary>
        /// <param name="includeMic">마이크도 녹음 할지의 여부</param>
        /// <returns>정상적으로 녹음이 되고 있는지의 여부</returns>
        public static bool Record(bool includeMic)
        {
            if (!IsInitialized || DefaultMMDeviceOut == null)
            {
                return false;
            }

            //기본 입력 장치를 바꾼 경우
            if (includeMic && _prevInDeviceId != DefaultMMDeviceIn.ID)
            {
                InitializeCaptureIn();
            }
            //기본 출력 장치를 바꾼 경우
            if (_prevOutDeviceId != DefaultMMDeviceOut.ID)
            {
                InitializeCaptureOut();
            }

            Stopwatch sw = new Stopwatch();

            void WhenDataAvailable(WaveInEventArgs e, WaveFormat format, ref Queue<WaveInEventArgs> waves)
            {
                sw.Stop();
                WaveInEventArgs waveIn = null;

                if (e.BytesRecorded == 0)
                {
                    int bytesPerMillisecond = format.AverageBytesPerSecond / 1000;
                    int bytesRecorded = (int)sw.ElapsedMilliseconds * bytesPerMillisecond;

                    byte[] buffer = new byte[bytesRecorded];
                    waveIn = new WaveInEventArgs(buffer, bytesRecorded);
                }
                else
                {
                    byte[] buffer = new byte[e.BytesRecorded];
                    Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesRecorded);

                    waveIn = new WaveInEventArgs(buffer, e.BytesRecorded);
                }
                waves.Enqueue(waveIn);

                sw.Reset();
                sw.Start();
            }

            _capture.DataAvailable += (s, e) =>
            {
                WhenDataAvailable(e, _capture.WaveFormat, ref _sounds);
            };
            if (includeMic) //마이크
            {
                _captureMic.DataAvailable += (s, e) =>
                {
                    WhenDataAvailable(e, _captureMic.WaveFormat, ref _soundsMic);
                };
            }

            _capture.StartRecording();
            if (includeMic)
            {
                _captureMic.StartRecording();
            }

            return true;
        }

        /// <summary>
        /// 소리 녹음을 중지하고 임시 파일에 파일을 저장합니다.
        /// </summary>
        /// <returns>성공적으로 중지되면 임시 파일 경로를 반환하고 그렇지 않으면 빈 문자열 (string.Empty)를 반환합니다.</returns>
        public static async Task<string> StopAsync()
        {
            return await StopAsync(MainWindow.GetTempFileName("mp3"));
        }

        /// <summary>
        /// 소리 녹음을 중지하고 HighlightInfo을 통해 파일을 저장합니다.
        /// </summary>
        /// <param name="info">하이라이트 정보</param>
        /// <returns>성공적으로 중지되면 파일 경로를 반환하고 그렇지 않으면 빈 문자열 (string.Empty)를 반환합니다.</returns>
        public static async Task<string> StopAsync(HighlightInfo info)
        {
            return await StopAsync(Highlight.GetAudioPath(info));
        }

        /// <summary>
        /// 소리 녹음을 중지하고 파일을 저장합니다.
        /// </summary>
        /// <param name="path">파일 경로</param>
        /// <returns>성공적으로 중지되면 파일 경로를 반환하고 그렇지 않으면 빈 문자열 (string.Empty)를 반환합니다.</returns>
        public static async Task<string> StopAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || _capture.CaptureState != CaptureState.Capturing)
            {
                return string.Empty;
            }

            var includeMic = _captureMic != null && _captureMic.CaptureState == CaptureState.Capturing;

            //소리 녹음 중지
            _capture.StopRecording();
            if (includeMic)
            {
                _captureMic.StopRecording();
            }

            string speakerPath = includeMic ? MainWindow.GetTempFileName("mp3") : path;

            //Queue에 있던 소리 데이터를 Mp3에 데이터 저장
            using (var writer = new LameMP3FileWriter(speakerPath, _capture.WaveFormat, 128))
            {
                while (_sounds.Count > 0)
                {
                    WaveInEventArgs waveIn = _sounds.Dequeue();
                    await writer.WriteAsync(waveIn.Buffer, 0, waveIn.BytesRecorded);
                }
            }

            //마이크를 녹음하지 않은 경우 끝났으므로 반환
            if (!includeMic)
            {
                return path;
            }

            string micPath = MainWindow.GetTempFileName("mp3");

            using (var writer = new LameMP3FileWriter(micPath, _captureMic.WaveFormat, 128))
            {
                while (_soundsMic.Count > 0)
                {
                    WaveInEventArgs waveIn = _soundsMic.Dequeue();
                    await writer.WriteAsync(waveIn.Buffer, 0, waveIn.BytesRecorded);
                }
            }

            MergeMp3(new[] {speakerPath, micPath}, path);

            //파일을 합쳤으므로 스피커 및 마이크 소리 파일 제거
            File.Delete(speakerPath);
            File.Delete(micPath);

            return path;
        }

        /// <summary>
        /// 여러 소리 파일을 한 파일로 합칩니다. (https://gist.github.com/gautamdhameja/e71317748e446bd85bb0875a0e64d6d5)
        /// </summary>
        /// <param name="paths">여러 소리 파일 경로</param>
        /// <param name="outPath">한 파일로 합칠 파일 경로</param>
        public static void MergeMp3(string[] paths, string outPath)
        {
            // Create a mixer object
            // This will be used for merging files together
            var mixer = new WaveMixerStream32
            {
                AutoStop = true
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    // create mp3 reader object
                    var reader = new Mp3FileReader(path);

                    // create a wave stream and a channel object
                    var waveStream = WaveFormatConversionStream.CreatePcmStream(reader);
                    var channel = new WaveChannel32(waveStream)
                    {
                        //Set the volume
                        Volume = 0.5f
                    };

                    // add channel as an input stream to the mixer
                    mixer.AddInputStream(channel);
                }
            }

            // convert wave stream from mixer to mp3
            var wave32 = new Wave32To16Stream(mixer);
            var mp3Writer = new LameMP3FileWriter(outPath, wave32.WaveFormat, 128);
            wave32.CopyTo(mp3Writer);

            // close all streams
            wave32.Close();
            mp3Writer.Close();
        }
    }
}