using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

// TODO get the f out
public class TeamManager : NetworkBehaviour
{
    [SyncVar]
    int BlueTeamScore;
    [SyncVar]
    int RedTeamScore;

    [SerializeField] TMP_Text blueTeamScoreText;
    [SerializeField] TMP_Text redTeamScoreText;

    [Command]
    public void CmdAddTeamScore(Team team, int score)
    {
        switch (team)
        {
            case Team.Blue:
                BlueTeamScore += score;
                break;
            case Team.Red:
                RedTeamScore += score;
                break;
        }
    }
}
