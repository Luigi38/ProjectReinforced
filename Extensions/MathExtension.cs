using System;

namespace ProjectReinforced.Extensions
{
    public static class MathExtension
    {
        /// <summary>
        /// 컴퓨터에서는 0.1 * 0.1 != 0.01 이라는 식이 성립할 때가 있다. (원래는 0.1 * 0.1 == 0.01 이어야 함)
        /// </summary>
        /// <param name="x">값1</param>
        /// <param name="y">값2</param>
        /// <returns></returns>
        public static bool EqualsPrecision(this double x, double y)
        {
            return Math.Abs(x - y) < 0.0000001;
        }
    }
}