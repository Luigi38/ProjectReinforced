using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using LCUSharp;
using LCUSharp.Websocket;

using ProjectReinforced.Types;

namespace ProjectReinforced.Clients
{
    /// <summary>
    /// for League of Legends
    /// </summary>
    public class LolClient : IGameClient
    {
        //상수 선언
        public GameType GAME_TYPE { get; } = GameType.Lol;
        public string PROCESS_NAME { get; } = "LeagueClient";

        public LeagueClientApi Client { get; set; }

        public Process GameProcess
        {
            get
            {
                var processes = Process.GetProcessesByName(PROCESS_NAME);

                if (processes.Length > 0) return processes[0];
                return null;
            }
        }

        public bool IsRunning => GameProcess != null;
        public bool IsActive => ClientManager.IsActive(PROCESS_NAME);
        public bool HasAssist => true;

        public int Kills => 0;
        public int Deaths => 0;
        public int Assists => 0;

        private event EventHandler<LeagueEvent> GameFlowChanged;

        public LolClient()
        {
            GameFlowChanged += OnGameFlowChanged;
        }

        public async Task InitializeAsync()
        {
            this.Client = await LeagueClientApi.ConnectAsync();
            this.Client.EventHandler.Subscribe("/lol-gameflow/v1/gameflow-phase", GameFlowChanged);

            // /lol-gameflow/v1/session
        }

        private void OnGameFlowChanged(object sender, LeagueEvent e)
        {
            Debug.WriteLine($"Status changed. '{e.Data}'");
        }
    }
}
