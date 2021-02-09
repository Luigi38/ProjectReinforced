using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LCUSharp;
using LCUSharp.Websocket;

namespace ProjectReinforced
{
    class LolClient
    {
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

        public LolClient()
        {
            this.Client = LeagueClientApi.ConnectAsync().Result;

            GameFlowChanged += OnGameFlowChanged;
            this.Client.EventHandler.Subscribe("/lol-gameflow/v1/gameflow-phase", GameFlowChanged);
        }

        private void OnGameFlowChanged(object sender, LeagueEvent e)
        {
            MessageBox.Show($"Status changed. '{e.Data}'");
        }
    }
}
