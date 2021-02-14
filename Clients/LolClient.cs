using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using LCUSharp;
using LCUSharp.Websocket;

namespace ProjectReinforced.Clients
{
    public class LolClient : IGameClient
    {
        private const string PROCESS_NAME = "CLIENT_NAME";

        /// <summary>
        /// 롤에서 킬을 할 때 오는 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void LolKillEventHandler(object sender, EventArgs e);

        public LeagueClientApi Client
        {
            get;
            set;
        }

        public event LolKillEventHandler OnKill;
        public event EventHandler<LeagueEvent> GameFlowChanged;

        public bool IsRunning
        {
            get
            {
                return GameClients.IsRunning(PROCESS_NAME);
            }
        }

        public LolClient()
        {
            GameFlowChanged += OnGameFlowChanged;
        }

        public async Task InitializeClientApi()
        {
            this.Client = await LeagueClientApi.ConnectAsync();
            this.Client.EventHandler.Subscribe("/lol-gameflow/v1/gameflow-phase", GameFlowChanged);
        }

        private void OnGameFlowChanged(object sender, LeagueEvent e)
        {
            MessageBox.Show($"Status changed. '{e.Data}'");
        }
    }
}
