using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using OpenCvSharp;
using OpenCvSharp.Extensions;

using MessagePack;

using ProjectReinforced.Extensions;

using Size = OpenCvSharp.Size;

namespace ProjectReinforced.Recording
{
    /// <summary>
    /// Desktop Duplication Api에 관한 특성을 효율적으로 사용하기 위해 만든 화면 캡처 저장 클래스
    /// </summary>
    public class ScreenCaptured
    {
        /// <summary>
        /// MessagePack을 위한 LZ4 압축 옵션
        /// </summary>
        public static MessagePackSerializerOptions LZ4_OPTIONS = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

        /// <summary>
        /// 현재 프레임 데이터
        /// </summary>
        private byte[] FrameData { get; set; }

        /// <summary>
        /// 프레임을 재사용할 횟수. 기본값은 1
        /// </summary>
        public int CountToUse { get; set; }
        
        /// <summary>
        /// 걸린 시간 (ms)
        /// </summary>
        public long ElapsedMilliseconds { get; set; }

        private Mat _frame;

        /// <summary>
        /// 현재 프레임
        /// </summary>
        public Mat Frame
        {
            get
            {
                if (_frame != null)
                {
                    return _frame;
                }

                _frame = new Mat(new Size(FrameWidth, FrameHeight), MatType.CV_8UC4);

                byte[] data = MessagePackSerializer.Deserialize<byte[]>(FrameData, LZ4_OPTIONS);
                Marshal.Copy(data, 0, _frame.Data, data.Length);

                return _frame;
            }
        }

        public Size FrameSize { get; }

        private int FrameWidth { get; }
        private int FrameHeight { get; }

        public ScreenCaptured(Bitmap frame, Size frameSize, long elapsedMilliseconds)
        {
            FrameData = MessagePackSerializer.Serialize(frame.ToArray(), LZ4_OPTIONS);
            FrameWidth = frame.Width;
            FrameHeight = frame.Height;
            FrameSize = frameSize;

            ElapsedMilliseconds = elapsedMilliseconds;
            CountToUse = 0;
        }

        public int GetCountToUseByElapsed(int fps)
        {
            int delay = 1000 / fps;
            return Math.Max((int)ElapsedMilliseconds / delay, 1); //스크린샷을 하는 데 걸린 시간을 기준으로 재활용 값 설정
        }
    }
}
