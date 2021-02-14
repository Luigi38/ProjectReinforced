using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectReinforced.Clients
{
    public class GameClients
    {
        public LolClient Lol { get; }
        public int R6 { get; }

        public GameClients()
        {
            this.Lol = new LolClient();
            this.R6 = 0;
        }

        public async Task Initialize()
        {
            if (Lol.IsRunning)
            {
                await Lol.InitializeClientApi();
            }
        }

        public static bool IsRunning(string name)
        {
            var processes = Process.GetProcessesByName(name);
            return processes.Length > 0;
        }
    }
}
