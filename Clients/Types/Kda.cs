namespace ProjectReinforced.Clients.Types
{
    public class Kda
    {
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }

        /// <summary>
        /// 해당 게임이 어시스트 기능이 있는가?
        /// </summary>
        public bool HasAssist { get; }

        public Kda(bool hasAssist)
        {
            Kills = 0;
            Deaths = 0;
            Assists = 0;
            HasAssist = hasAssist;
        }

        public Kda(int kills, int deaths, int assists, bool hasAssist)
        {
            Kills = kills;
            Deaths = deaths;
            Assists = assists;
            HasAssist = hasAssist;
        }

        public override string ToString()
        {
            int[] kda = HasAssist ? new[] {Kills, Deaths, Assists} : new[] {Kills, Deaths};
            return string.Join("/", kda);
        }
    }
}
