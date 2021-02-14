using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectReinforced.Clients
{
    interface IGameClient
    {
        /// <summary>
        /// 그 게임이 현재 실행 되어있는가?
        /// </summary>
        bool IsRunning
        {
            get;
        }
    }
}
