using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectReinforced.Recording
{
    public class HighlightManager
    {
        public static List<Highlight> Highlights { get; } = new List<Highlight>();
        public static string LocalPath { get; set; }
    }
}
