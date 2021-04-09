using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectReinforced.Clients.Types;

namespace ProjectReinforced.Clients
{
    /// <summary>
    /// 게임 클라이언트 API 기초 인터페이스
    /// </summary>
    public interface IGameClient
    {
        /// <summary>
        /// 게임 종류
        /// </summary>
        GameType GAME_TYPE { get; }
        /// <summary>
        /// 게임 프로세스 이름
        /// </summary>
        string PROCESS_NAME { get; }
        /// <summary>
        /// 게임 프로세스 제목
        /// </summary>
        string PROCESS_TITLE { get; }

        /// <summary>
        /// 그 게임이 현재 실행 되어있는가?
        /// </summary>
        bool IsRunning { get; }
        /// <summary>
        /// 그 게임을 현재 플레이 하고 있는가? (활성화 상태)
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// 클라이언트 API가 초기화 되어있는가?
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 킬/데스/어시스트
        /// </summary>
        Kda Statistics { get; }

        /// <summary>
        /// 현재 게임 프로세스
        /// </summary>
        Process GameProcess { get; }

        /// <summary>
        /// 비동기로 클라이언트 API를 초기화합니다.
        /// </summary>
        /// <returns></returns>
        Task InitializeAsync();
    }
}
