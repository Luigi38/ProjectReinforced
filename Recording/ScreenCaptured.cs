using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

using OpenCvSharp;

using ProjectReinforced.Extensions;

namespace ProjectReinforced.Recording
{
    /// <summary>
    /// Desktop Duplication Api에 관한 특성을 효율적으로 사용하기 위해 만든 화면 캡처 저장 클래스
    /// </summary>
    [Serializable]
    public class ScreenCaptured
    {
        /// <summary>
        /// 현재 프레임
        /// </summary>
        public Mat Frame { get; }

        /// <summary>
        /// 프레임을 재사용할 횟수. 기본값은 1
        /// </summary>
        public int CountToUse { get; set; }
        
        /// <summary>
        /// 걸린 시간 (ms)
        /// </summary>
        public long ElapsedMilliseconds { get; set; }

        public ScreenCaptured(Mat frame, long elapsedMilliseconds)
        {
            Frame = frame;
            ElapsedMilliseconds = elapsedMilliseconds;
            CountToUse = 1;
        }
    }
}
