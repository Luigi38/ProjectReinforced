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

using ProjectReinforced.Clients;
using ProjectReinforced.Recording;
using ProjectReinforced.Types;
using ProjectReinforced.Others;

namespace ProjectReinforced.Clients.Lol
{
    /// <summary>
    /// for League of Legends
    /// </summary>
    public class LolClient : IGameClient
    {
        //상수 선언
        public GameType GAME_TYPE { get; } = GameType.Lol;
        public string PROCESS_NAME { get; } = "League of Legends";
        public string PROCESS_TITLE { get; } = "League of Legends (TM) Client";

        public LeagueClientApi Client { get; set; }

        public Process GameProcess //인게임 프로세스
        {
            get
            {
                var processes = Process.GetProcessesByName(PROCESS_NAME);
                return processes.Length > 0 ? processes[0] : null;
            }
        }

        public bool IsRunning => GameProcess != null;
        public bool IsActive => ClientManager.IsActive(PROCESS_TITLE);
        public bool HasAssist => true;
        public bool IsInitialized { get; private set; }

        public int Kills { get; private set; }
        public int Deaths { get; private set; }
        public int Assists { get; private set; }

        private LolLiveEvent _latestEvent = LolLiveEvent.Empty;

        public event EventHandler OnKill;
        public event EventHandler OnDeath;
        public event EventHandler OnAssist;

        public LolSummoner CurrentSummoner { get; private set; }

        public LolClient()
        {
            OnKill += LolClient_OnKill;
            OnDeath += LolClient_OnDeath;
            OnAssist += LolClient_OnAssist;
        }

        public async Task InitializeAsync()
        {
            this.Client = await LeagueClientApi.ConnectAsync();

            _ = Task.Run(RequestData); //이벤트 관리
            IsInitialized = true;
        }

        private void LolClient_OnKill(object sender, EventArgs e)
        {
            Kills++;

            MessageBox.Show($"KILLED! : {Kills}/{Deaths}/{Assists}");

            HighlightInfo info = new HighlightInfo("PentaKill", DateTime.Now, GAME_TYPE);
            Highlight kill = Screen.Stop(info);

            if (kill != null)
            {
                HighlightManager.AddHighlight(kill);
            }
        }

        private void LolClient_OnDeath(object sender, EventArgs e)
        {
            Deaths++;
            MessageBox.Show($"DEATHED. : {Kills}/{Deaths}/{Assists}");
        }

        private void LolClient_OnAssist(object sender, EventArgs e)
        {
            Assists++;
        }

        private async void RequestData()
        {
            try
            {
                await RequestCurrentSummonerAsync(); //플레이어 정보 가져오기

                using (HttpClient httpClient = new HttpClient())
                {
                    //Http 클라이언트 초깃값 설정
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                           SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    httpClient.DefaultRequestHeaders.Host = "127.0.0.1:2999";
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:85.0) Gecko/20100101 Firefox/85.0");

                    //무한반복문
                    while (IsRunning)
                    {
                        if (IsActive)
                        {
                            if (await GetGameflowPhaseAsync() == GameflowPhaseType.InProgress) //현재 롤을 플레이 중인 경우
                            {
                                //실시간 이벤트 데이터 불러오기
                                var message = await httpClient.GetAsync("https://127.0.0.1:2999/liveclientdata/eventdata");
                                var content = await message.Content.ReadAsStringAsync();

                                //이벤트 배열 가져오기
                                JObject obj = JObject.Parse(content);
                                JArray array = JArray.Parse(obj["Events"]?.ToString() ?? string.Empty);

                                //챔피언 킬 데이터 이벤트 배열만 가져오기
                                JToken[] tokens = array.Where(token => token["EventName"]?.ToString() == "ChampionKill").ToArray();

                                foreach (var token in tokens)
                                {
                                    var currentEvent = token.ToObject<LolLiveEvent>(); //이벤트 데이터 클래스로 변환
                                    
                                    //새로운 이벤트가 일어난 경우
                                    if (currentEvent != null && currentEvent.EventTime > _latestEvent.EventTime)
                                    {
                                        string userName = CurrentSummoner?.displayName;
                                        _latestEvent = currentEvent;

                                        if (!string.IsNullOrEmpty(userName) && currentEvent.KillerName == userName) //킬
                                        {
                                            OnKill?.Invoke(currentEvent, new EventArgs());
                                        }
                                        else if (!string.IsNullOrEmpty(userName) && currentEvent.VictimName == userName) //죽음
                                        {
                                            OnDeath?.Invoke(currentEvent, new EventArgs());
                                        }
                                        else if (!string.IsNullOrEmpty(userName) && currentEvent.Assisters.Contains(userName)) //어시스트
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

        /// <summary>
        /// 비동기로 현재 플레이어 정보를 http로 요청합니다. 플레이어 정보는 CurrentSummoner 프로퍼티에 저장됩니다.
        /// </summary>
        private async Task RequestCurrentSummonerAsync()
        {
            string json = await Client.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, "/lol-summoner/v1/current-summoner");
            CurrentSummoner = JsonConvert.DeserializeObject<LolSummoner>(json);
        }

        /// <summary>
        /// 현재 게임 플레이 상태를 가져옵니다.
        /// </summary>
        /// <returns>현재 게임 플레이 상태</returns>
        public async Task<GameflowPhaseType> GetGameflowPhaseAsync()
        {
            var response = await Client.RequestHandler.GetResponseAsync<string>(HttpMethod.Get,
                "/lol-gameflow/v1/gameflow-phase");

            Enum.TryParse(response, out GameflowPhaseType phase);
            return phase;
        }
    }
}
