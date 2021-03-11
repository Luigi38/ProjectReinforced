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

        private const string SESSION_URL = "https://127.0.0.1:2999/liveclientdata/eventdata";
        private const string SESSION_HOST = "127.0.0.1:2999";

        private const string USER_AGENT =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:85.0) Gecko/20100101 Firefox/85.0";

        /// <summary>
        /// 리그 오브 레전드 클라이언트 API
        /// </summary>
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
        public bool IsInitialized { get; private set; }

        public Kda Kda { get; private set; }

        private LolLiveEvent _latestEvent = LolLiveEvent.Empty;

        public event EventHandler<Kda> OnKill;
        public event EventHandler<Kda> OnDeath;
        public event EventHandler<Kda> OnAssist;
        public event EventHandler OnSessionStart;
        public event EventHandler OnSessionEnd;

        public LolSummoner CurrentSummoner { get; private set; }

        public LolClient()
        {
            Kda = new Kda(true);

            OnKill += LolClient_OnKill;
            OnDeath += LolClient_OnDeath;
            OnAssist += LolClient_OnAssist;

            OnSessionStart += LolClient_OnSessionStart;
            OnSessionEnd += LolClient_OnSessionEnd;
        }

        public async Task InitializeAsync()
        {
            this.Client = await LeagueClientApi.ConnectAsync();

            _ = Task.Run(RequestData); //이벤트 관리
            IsInitialized = true;
        }

        private void LolClient_OnKill(object sender, Kda kda)
        {
            HighlightInfo info = new HighlightInfo("Kill", DateTime.Now, GAME_TYPE);
            Highlight kill = Screen.Stop(info);

            if (kill != null)
            {
                HighlightManager.AddHighlight(kill);
            }

            Trace.WriteLine($"KILLED! ({kda})");
        }

        private void LolClient_OnDeath(object sender, Kda kda)
        {
            Trace.WriteLine($"DEATHED. ({kda})");
        }

        private void LolClient_OnAssist(object sender, Kda kda)
        {
            Trace.WriteLine($"ASSISTED. ({kda})");
        }

        private void LolClient_OnSessionStart(object sender, EventArgs e)
        {
            
        }

        private void LolClient_OnSessionEnd(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// 리그 오브 레전드 클라이언트 API에서 해당 게임 데이터를 가져옵니다.
        /// </summary>
        private async void RequestData()
        {
            try
            {
                await RequestCurrentSummonerAsync(); //플레이어 정보 가져오기
                bool isRequesting = false;

                using (HttpClient httpClient = new HttpClient())
                {
                    //Http 클라이언트 초깃값 설정
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                           SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    httpClient.DefaultRequestHeaders.Host = SESSION_HOST;
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(USER_AGENT);

                    //무한반복문
                    while (true)
                    {
                        if (!isRequesting && IsRunning) //첫 이벤트
                        {
                            isRequesting = true;
                            OnSessionStart?.Invoke(this, new EventArgs());
                        }

                        while (IsRunning)
                        {
                            if (IsActive)
                            {
                                if (await GetGameflowPhaseAsync() == GameflowPhaseType.InProgress) //현재 롤을 플레이 중인 경우
                                {
                                    //실시간 이벤트 데이터 불러오기
                                    var message = await httpClient.GetAsync(SESSION_URL);
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

                                            if (!string.IsNullOrEmpty(userName))
                                            {
                                                if (currentEvent.KillerName == userName) //킬
                                                {
                                                    Kda.Kills++;
                                                    OnKill?.Invoke(currentEvent, Kda);
                                                }
                                                else if (currentEvent.VictimName == userName) //죽음
                                                {
                                                    Kda.Deaths++;
                                                    OnDeath?.Invoke(currentEvent, Kda);
                                                }
                                                else if (currentEvent.Assisters.Contains(userName)) //어시스트
                                                {
                                                    Kda.Assists++;
                                                    OnAssist?.Invoke(currentEvent, Kda);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            await Task.Delay(1000);
                        }

                        if (isRequesting && !IsRunning)
                        {
                            isRequesting = false;
                            OnSessionEnd?.Invoke(this, new EventArgs());
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
