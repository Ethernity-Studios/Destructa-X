using UnityEngine;
using Mirror;

public enum Team
{
    Blue, Red
}

public class Player : NetworkBehaviour
{
    public string DisplayName;
    public Team SelectedTeam;

    private void Start()
    {
        DisplayName = NicknameManager.DisplayName;
    }
}
