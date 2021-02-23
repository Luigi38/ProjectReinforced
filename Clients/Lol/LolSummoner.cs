using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectReinforced.Clients.Lol
{
    public class LolSummoner
    {
        public string accountId { get; }
        public string displayName { get; }
        public string internalName { get; } //솔직히 displayName이랑 무슨 다른 점이 있는지 모르겠음.
        public string summonerId { get; }
        public int summonerLevel { get; }

        public LolSummoner(string accountId, string displayName, string internalName, string summonerId,
            int summonerLevel)
        {
            this.accountId = accountId;
            this.displayName = displayName;
            this.internalName = internalName;
            this.summonerId = summonerId;
            this.summonerLevel = summonerLevel;
        }
    }
}
