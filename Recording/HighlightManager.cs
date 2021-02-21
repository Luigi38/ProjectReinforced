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

        public static void AddHighlight(GameType game, Highlight highlight)
        {
            if (!Highlights.ContainsKey(game))
            {
                Highlights.Add(game, new List<Highlight>());
            }

            Highlights[game].Add(highlight);
        }

        public static List<Highlight> GetHighlights(GameType game)
        {
            var containsValue = Highlights.TryGetValue(game, out var highlights);
            return containsValue ? highlights : null;
        }
    }
}
