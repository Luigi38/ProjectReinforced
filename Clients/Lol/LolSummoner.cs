using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectReinforced.Clients.Lol
{
    public class LolSummoner
    {
        public decimal accountId { get; }
        public string displayName { get; }
        public string internalName { get; } //솔직히 displayName이랑 무슨 다른 점이 있는지 모르겠음.
        public decimal summonerId { get; }
        public int summonerLevel { get; }
    }
}
