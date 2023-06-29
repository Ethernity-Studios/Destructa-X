using UnityEngine;
using Mirror;
using System.Collections;

public class BombManager : NetworkBehaviour
{
    [SerializeField] GameObject bombExplosion;
    private UIManager uiManager;

    [SerializeField] bool detonating;
    [SyncVar] public bool canBoom = true;

    private bool isTicked;

    [Command(requiresAuthority = false)]
    private void CmdDetonateBomb()
    {
        if (!canBoom) return;
        if (detonating) return;
        detonating = true;
        GameObject explosion = Instantiate(bombExplosion);
        NetworkServer.Spawn(explosion);
        RpcSetupExplosion(explosion);
    }
    [ClientRpc]
    void RpcSetupExplosion(GameObject explosion)
    {
        
        explosion.transform.SetParent(transform);
        explosion.transform.localScale = new Vector3(.1f, .1f, .1f);
        explosion.transform.position = transform.position;
    }
    private void Start()
    {
        StartCoroutine(startBombTimer());
        StartCoroutine(StartTicking());
    }

    IEnumerator startBombTimer()
    {
        yield return new WaitForSeconds(40);
        CmdDetonateBomb();
    }

    [ClientRpc]
    public void StopExplosion()
    {
        StopAllCoroutines();
    }

    public IEnumerator StartTicking()
    {
        uiManager = FindObjectOfType<UIManager>();
        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForSeconds(1); //20 sec
            Tick();
        }
        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForSeconds(.5f); // 10 sec
            Tick();
        }
        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForSeconds(.25f); // 5 sec
            Tick();
        }
        for (int i = 0; i < 40; i++)
        {
            yield return new WaitForSeconds(.125f); // 5 sec
            Tick();
        }
    }

    public void Tick()
    {
        isTicked = !isTicked;
        uiManager.PlantedSpike.color = isTicked ? new Color(0.84f, 0.27f, 0.27f) : new Color(0.84f, 0.18f, 0.14f);
        //Play sound
    }
}
