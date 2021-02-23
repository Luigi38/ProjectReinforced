using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectReinforced.Clients.Lol
{
    class LolLiveEvent
    {
        //https://developer.riotgames.com/docs/lol#league-client-api

        public string[] Assisters { get; }

        public int EventID { get; }
        public string EventName { get; }
        public double EventTime { get; }

        public string KillerName { get; }
        public string VictimName { get; }

        public static LolLiveEvent Empty =>
            new LolLiveEvent(new string[] { }, 0, string.Empty, 0.0, string.Empty, string.Empty);

        public LolLiveEvent(string[] assisters, int eventId, string eventName, double eventTime, string killerName, string victimName)
        {
            Assisters = assisters;

            EventID = eventId;
            EventName = eventName;
            EventTime = eventTime;

            KillerName = killerName;
            VictimName = victimName;
        }
    }
}
