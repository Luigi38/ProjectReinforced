using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectReinforced.Types;

namespace ProjectReinforced.Clients
{
    /// <summary>
    /// for Rainbow Six Siege
    /// </summary>
    public class RainbowClient : IGameClient
    {
        public GameType GAME_TYPE { get; } = GameType.R6;
        public string PROCESS_NAME { get; } = "RainbowSix";

        public int Client { get; set; }

        public Process GameProcess
        {
            get
            {
                var processes = Process.GetProcessesByName(PROCESS_NAME);
                return processes.Length > 0 ? processes[0] : null;
            }
        }

        public bool IsRunning => GameProcess != null;
        public bool IsActive => ClientManager.IsActive(PROCESS_NAME);
        public bool HasAssist => true;

        public int Kills => 0;
        public int Deaths => 0;
        public int Assists => 0;

        public RainbowClient()
        {
            this.Client = 0;
        }

        public async Task InitializeAsync()
        {
            this.Client = 0;
            await Task.Delay(10);
        }
    }
}
