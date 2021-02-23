using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Net.Http;

using LCUSharp;
using LCUSharp.Websocket;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public string PROCESS_TITLE { get; } = "League of Legends (TM) Client";

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
        public bool IsActive => ClientManager.IsActive(PROCESS_TITLE);
        public bool HasAssist => true;
        public bool IsInitialized { get; private set; }

        public int Kills { get; private set; } = 0;
        public int Deaths { get; private set; } = 0;
        public int Assists { get; private set; } = 0;

        private event EventHandler<LeagueEvent> GameFlowChanged;
        private LolLiveEvent _latestEvent = LolLiveEvent.Empty;

        public event EventHandler OnKill;
        public event EventHandler OnDeath;
        public event EventHandler OnAssist;

        public LolClient()
        {
            GameFlowChanged += OnGameFlowChanged;

            OnKill += LolClient_OnKill;
            OnDeath += LolClient_OnDeath;
            OnAssist += LolClient_OnAssist;
        }

        public async Task InitializeAsync()
        {
            this.Client = await LeagueClientApi.ConnectAsync();
            this.Client.EventHandler.Subscribe("/lol-gameflow/v1/gameflow-phase", GameFlowChanged);

            _ = Task.Run(RequestData);
            IsInitialized = true;
        }

        private void OnGameFlowChanged(object sender, LeagueEvent e)
        {
            Debug.WriteLine($"Status changed. '{e.Data}'");
        }

        private async void RequestData()
        {
            try
            {
                string json = await this.Client.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, "/lol-summoner/v1/current-summoner");
                Clipboard.SetText(json);

                using (HttpClient httpClient = new HttpClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                           SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    httpClient.DefaultRequestHeaders.Host = "127.0.0.1:2999";
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:85.0) Gecko/20100101 Firefox/85.0");

                    while (IsRunning)
                    {
                        if (IsActive)
                        {
                            var message = await httpClient.GetAsync("https://127.0.0.1:2999/liveclientdata/eventdata");
                            var content = await message.Content.ReadAsStringAsync();

                            JObject obj = JObject.Parse(content);
                            JArray array = JArray.Parse(obj["Events"]?.ToString() ?? string.Empty);

                            foreach (var itemObj in array)
                            {
                                string eventName = itemObj["EventName"]?.ToString();

                                if (eventName == "ChampionKill")
                                {
                                    var currentEvent = itemObj.ToObject<LolLiveEvent>();

                                    if (currentEvent != null && currentEvent.EventTime > _latestEvent.EventTime)
                                    {
                                        string userName = "파이수학엄마";
                                        _latestEvent = currentEvent;

                                        if (currentEvent.KillerName == userName) //킬
                                        {
                                            OnKill?.Invoke(currentEvent, new EventArgs());
                                        }
                                        else if (currentEvent.VictimName == userName) //죽음
                                        {
                                            OnDeath?.Invoke(currentEvent, new EventArgs());
                                        }
                                        else if (currentEvent.Assisters.Contains(userName)) //어시스트
                                        {
                                            OnAssist?.Invoke(currentEvent, new EventArgs());
                                        }
                                    }
                                }
                            }
                        }

                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"오류 발생.\n{e}");
            }
        }

        private void LolClient_OnKill(object sender, EventArgs e)
        {
            Kills++;
            Debug.WriteLine("KILLED!");
        }

        private void LolClient_OnDeath(object sender, EventArgs e)
        {
            Deaths++;
            Debug.WriteLine("DEATHED.");
        }

        private void LolClient_OnAssist(object sender, EventArgs e)
        {
            Assists++;
        }
    }
}
