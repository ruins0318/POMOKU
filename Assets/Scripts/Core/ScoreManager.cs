using Pomoku.Board;
using UnityEngine;

namespace Pomoku.Core
{
    public class ScoreManager : MonoBehaviour
    {
        private int teamAScore;
        private int teamBScore;

        public void ResetScores()
        {
            teamAScore = 0;
            teamBScore = 0;
        }

        public void AddScore(TeamId teamId, int score)
        {
            if (teamId == TeamId.TeamA)
            {
                teamAScore += score;
                return;
            }

            if (teamId == TeamId.TeamB)
            {
                teamBScore += score;
            }
        }

        public int GetScore(TeamId teamId)
        {
            if (teamId == TeamId.TeamA)
            {
                return teamAScore;
            }

            if (teamId == TeamId.TeamB)
            {
                return teamBScore;
            }

            return 0;
        }
    }
}
