using UnityEngine;
using Mirror;

public class NetworkPlayer : NetworkBehaviour
{
    public string Name;

    NicknameManager nicknameManager;
    private void Start()
    {
        nicknameManager = FindObjectOfType<NicknameManager>();
        Name = nicknameManager.GetNickName();
        Debug.Log("my epic nicknam");
    }
}
