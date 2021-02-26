using System;
using System.Windows;

namespace ProjectReinforced.Others
{
    public class ExceptionManager
    {
        /// <summary>
        /// 오류를 로그에 기록합니다.
        /// </summary>
        /// <param name="message">오류 내용</param>
        /// <param name="title">메시지 제목</param>
        public static void WriteLog(string message, string title)
        {
            WriteLogInternal(message, title, string.Empty);
        }

        /// <summary>
        /// 오류를 로그에 기록합니다.
        /// </summary>
        /// <param name="message">오류 내용</param>
        /// <param name="title">메시지 제목</param>
        /// <param name="className">클래스 이름</param>
        /// <param name="methodName">함수 이름</param>
        public static void WriteLog(string message, string title, string className, string methodName)
        {
            WriteLogInternal(message, title, $"{className}.{methodName}()");
        }

        private static void WriteLogInternal(string message, string title, string fullMethodName)
        {
        }

        /// <summary>
        /// 오류를 메시지 박스로만 표시하고 로그에 기록합니다.
        /// </summary>
        /// <param name="message">오류 내용</param>
        /// <param name="title">메시지 박스 제목</param>
        public static void ShowError(string message, string title)
        {
            ShowError(message, title, string.Empty, string.Empty);
        }

        /// <summary>
        /// 오류를 메시지 박스로만 표시하고 로그에 기록합니다.
        /// </summary>
        /// <param name="message">오류 내용</param>
        /// <param name="title">메시지 박스 제목</param>
        /// <param name="className">클래스 이름</param>
        /// <param name="methodName">함수 이름</param>
        public static void ShowError(string message, string title, string className, string methodName)
        {
            string fullMethodName = !string.IsNullOrWhiteSpace(className) && !string.IsNullOrWhiteSpace(methodName) ? $"{className}.{methodName}()" : string.Empty;
            string finalMessage = !string.IsNullOrEmpty(fullMethodName) ? $"{message}\nat {fullMethodName}" : message;

            WriteLogInternal(message, title, fullMethodName);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}