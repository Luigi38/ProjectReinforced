using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectReinforced.Types;

namespace ProjectReinforced.Recording
{
    public class HighlightManager
    {
        public static Dictionary<GameType, List<Highlight>> Highlights { get; } = new Dictionary<GameType, List<Highlight>>();
        
        public static string LocalPath { get; set; }

        public static void AddHighlight(Highlight highlight)
        {
            if (!Highlights.ContainsKey(highlight.Info.Game))
            {
                Highlights.Add(highlight.Info.Game, new List<Highlight>());
            }

            Highlights[highlight.Info.Game].Add(highlight);
        }

        public static List<Highlight> GetHighlights(GameType game)
        {
            var containsValue = Highlights.TryGetValue(game, out var highlights);
            return containsValue ? highlights : null;
        }
    }
}
