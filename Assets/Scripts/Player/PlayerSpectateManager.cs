using UnityEngine;

public class PlayerSpectateManager : MonoBehaviour
{
    Player player;
    private void Start()
    {
        player = GetComponent<Player>();    
    }

    private void Update()
    {
        if (player.IsDeath) return;
    }
}
