using System;
using System.IO;

using ProjectReinforced.Types;

namespace ProjectReinforced.Recording
{
    public class Highlight
    {
        public HighlightInfo Info { get; }
        public string VideoPath => GetVideoPath(Info);

        public Highlight(HighlightInfo info)
        {
            this.Info = info;
        }

        public static string GetHighlightFileName(DateTime date)
        {
            return GetHighlightFileName(date, string.Empty);
        }

        public static string GetHighlightFileName(DateTime date, string extension)
        {
            string fileName = date.ToString("yy-MM-dd-HH-mm-ss");
            return string.IsNullOrWhiteSpace(extension) ? fileName : string.Join(".", fileName, extension);
        }

        public static string GetVideoPath(HighlightInfo info)
        {
            return GetFilePath(info.Game, GetHighlightFileName(info.EventDate, "mp4"));
        }

        public static string GetAudioPath(HighlightInfo info)
        {
            return GetFilePath(info.Game, GetHighlightFileName(info.EventDate, "mp3"));
        }

        public static string GetFilePath(GameType gameType, string fileName)
        {
            return Path.Combine(HighlightManager.LocalPath, gameType.ToString(), fileName);
        }
    }
}
