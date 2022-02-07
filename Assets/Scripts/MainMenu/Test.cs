using UnityEngine;
using Mirror;

public class Test : NetworkBehaviour
{
    void Start()
    {
        Debug.Log(NicknameManager.DisplayName);
    }

    void Update()
    {

    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
    }
}
