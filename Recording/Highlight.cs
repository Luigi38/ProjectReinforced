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
            return date.ToString("yy-MM-dd-HH-mm-ss");
        }

        public static string GetVideoPath(HighlightInfo info)
        {
            return GetVideoPath(info.Game, $"{GetHighlightFileName(info.EventDate)}.mp4");
        }

        public static string GetVideoPath(GameType gameType, string fileName)
        {
            return Path.Combine(HighlightManager.LocalPath, gameType.ToString(), fileName);
        }
    }
}
