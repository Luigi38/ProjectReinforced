using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectReinforced.Clients.Types;

namespace ProjectReinforced.Recording.Types
{
    public class HighlightInfo
    {
        public string EventName { get; }
        public DateTime EventDate { get; }

        public GameType Game { get; }
        public string GameName { get; }

        public HighlightInfo(string eventName, DateTime eventDate, GameType game, string gameName)
        {
            this.EventName = eventName;
            this.EventDate = eventDate;

            this.Game = game;
            this.GameName = gameName;
        }

        public HighlightInfo(string eventName, DateTime eventDate, GameType game) : this(eventName, eventDate,
            game, game.ToString())
        {

        }
    }
}
