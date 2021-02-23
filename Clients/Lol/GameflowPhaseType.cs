using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectReinforced.Clients.Lol
{
    public enum GameflowPhaseType
    {
        None,
        Lobby,
        Matchmaking,
        CheckedIntoTournament,
        ReadyCheck,
        ChampSelect,
        GameStart,
        FailedToLaunch,
        InProgress,
        Reconnect,
        WaitingForStats,
        PreEndOfGame,
        EndOfGame,
        TerminatedInError
    }
}
