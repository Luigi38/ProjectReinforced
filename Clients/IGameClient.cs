using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectReinforced.Clients
{
    public interface IGameClient
    {
        /// <summary>
        /// 그 게임이 현재 실행 되어있는가?
        /// </summary>
        bool IsRunning
        {
            get;
        }

        /// <summary>
        /// 현재 게임 프로세스
        /// </summary>
        Process GameProcess
        {
            get;
        }
    }
}
