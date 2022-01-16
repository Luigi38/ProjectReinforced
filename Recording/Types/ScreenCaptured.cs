using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using OpenCvSharp;
using OpenCvSharp.Extensions;

using MessagePack;

using ProjectReinforced.Extensions;

using Size = OpenCvSharp.Size;

namespace ProjectReinforced.Recording.Types
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
        private byte[] FrameData { get; }
        
        /// <summary>
        /// 현재 프레임의 원하는 크기
        /// </summary>
        public Size FrameSize { get; }

        /// <summary>
        /// 시작한 시간
        /// </summary>
        public DateTime NowStart { get; set; }

        /// <summary>
        /// 끝난 시간
        /// </summary>
        public DateTime NowEnd { get; set; }

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

                byte[] data = MessagePackSerializer.Deserialize<byte[]>(FrameData, LZ4_OPTIONS);
                _frame = Cv2.ImDecode(data, ImreadModes.Color);

                return _frame;
            }
        }

        public ScreenCaptured(Mat frame, Size frameSize, DateTime time)
        {
            byte[] data = frame.ToBytes(".jpg");

            FrameData = MessagePackSerializer.Serialize(data, LZ4_OPTIONS);
            FrameSize = frameSize;

            NowStart = time;
        }
    }
}
