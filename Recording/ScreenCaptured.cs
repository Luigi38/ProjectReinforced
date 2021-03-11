using OpenCvSharp;

namespace ProjectReinforced.Recording
{
    /// <summary>
    /// Desktop Duplication Api에 관한 특성을 효율적으로 사용하기 위해 만든 화면 캡처 저장 클래스
    /// </summary>
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

        public ScreenCaptured(Mat frame)
        {
            Frame = frame;
            CountToUse = 1;
        }

        public ScreenCaptured(Mat frame, int countToUse)
        {
            Frame = frame;
            CountToUse = countToUse;
        }
    }
}
