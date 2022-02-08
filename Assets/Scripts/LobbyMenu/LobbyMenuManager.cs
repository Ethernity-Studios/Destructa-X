using UnityEngine;
using Mirror;
using TMPro;

public class LobbyMenuManager : NetworkBehaviour
{
    [SerializeField] GameObject selectTeamUI;
    [SerializeField] GameObject agentSelectUI;

    [SyncVar(hook = nameof(UpdateBlueTeamSize))]
    public int BlueTeamSize;
    [SyncVar(hook = nameof(UpdateRedTeamSize))]
    public int RedTeamSize;
    [SerializeField] TMP_Text BlueTeamCountBtnText;
    [SerializeField] TMP_Text RedTeamCountBtnText;

    private void Start()
    {
        BlueTeamCountBtnText.text = BlueTeamSize.ToString();
        RedTeamCountBtnText.text = RedTeamSize.ToString();
    }

    public void JoinTeam(int teamIndex)
    {
        LobbyPlayer.SelectedTeam = (Team)teamIndex;
        selectTeamUI.SetActive(false);
        agentSelectUI.SetActive(true);

        if (teamIndex == 0)
        {
            BlueTeamSize++;
            BlueTeamCountBtnText.text = BlueTeamSize.ToString();
        }
        else if (teamIndex == 1)
        {
            RedTeamSize++;
            RedTeamCountBtnText.text = RedTeamSize.ToString();
        }
    }

    void UpdateBlueTeamSize(int _, int newValue)
    {
        BlueTeamCountBtnText.text = newValue.ToString();
    }

    void UpdateRedTeamSize(int _, int newValue)
    {
        BlueTeamCountBtnText.text = newValue.ToString();
    }
}
