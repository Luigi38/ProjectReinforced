using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using OpenCvSharp;
using OpenCvSharp.Extensions;

using K4os.Compression.LZ4;

using ProjectReinforced.Extensions;

using Size = OpenCvSharp.Size;

namespace ProjectReinforced.Recording
{
    /// <summary>
    /// Desktop Duplication Api에 관한 특성을 효율적으로 사용하기 위해 만든 화면 캡처 저장 클래스
    /// </summary>
    public class ScreenCaptured
    {
        private readonly byte[] _frameData;
        private readonly int _frameDataLength;
        private readonly int _frameWidth;
        private readonly int _frameHeight;
        private readonly PixelFormat _framePixelFormat;

        private Mat _frame;

        /// <summary>
        /// 현재 프레임 (압축 해제 -> 비트맵으로 변환 -> Mat으로 변환)
        /// </summary>
        public Mat Frame
        {
            get
            {
                if (_frame != null) return _frame;

                byte[] data = new byte[_frameDataLength];
                LZ4Codec.Decode(_frameData, 0, _frameData.Length, data, 0, data.Length);

                Bitmap bitmap = data.ToBitmap(_frameWidth, _frameHeight, _framePixelFormat);

                _frame = bitmap.ToMat();
                bitmap.Dispose();

                return _frame;
            }
        }

        /// <summary>
        /// 프레임을 재사용할 횟수. 기본값은 1
        /// </summary>
        public int CountToUse { get; set; }
        
        /// <summary>
        /// 걸린 시간 (ms)
        /// </summary>
        public long ElapsedMilliseconds { get; set; }

        public Size FrameSize { get; }

        public ScreenCaptured(Bitmap frame, Size frameSize, long elapsedMilliseconds)
        {
            byte[] sourceData = frame.ToArray();

            _frameData = new byte[LZ4Codec.MaximumOutputSize(sourceData.Length)];
            LZ4Codec.Encode(sourceData, 0, sourceData.Length, _frameData, 0, _frameData.Length);

            _frameDataLength = sourceData.Length;
            _frameWidth = frame.Width;
            _frameHeight = frame.Height;
            _framePixelFormat = frame.PixelFormat;
            FrameSize = frameSize;

            ElapsedMilliseconds = elapsedMilliseconds;
            CountToUse = 1;
        }
    }
}
