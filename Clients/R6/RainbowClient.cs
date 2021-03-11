using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectReinforced.Clients;
using ProjectReinforced.Types;

namespace ProjectReinforced.Clients.R6
{
    /// <summary>
    /// for Rainbow Six Siege
    /// </summary>
    public class RainbowClient : IGameClient
    {
        public GameType GAME_TYPE { get; } = GameType.R6;
        public string PROCESS_NAME { get; } = "RainbowSix";
        public string PROCESS_TITLE { get; } = "Rainbow Six Siege";

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
        public bool IsInitialized { get; private set; }

        public Kda Kda { get; private set; }

        public RainbowClient()
        {
            this.Client = 0;
            this.Kda = new Kda(true);
        }

        public async Task InitializeAsync()
        {
            this.Client = 0;
            await Task.Delay(10);

            IsInitialized = true;
        }
    }
}
