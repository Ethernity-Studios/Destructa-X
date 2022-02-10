using Mirror;
using UnityEngine;

namespace player
{
    public class PlayerDebug : NetworkBehaviour
    {
        public void Update()
        {
            // if (!isLocalPlayer) return;

            if (Input.GetKeyDown(KeyCode.C))
                // rend.material.color = Color.yellow;
                ChangColor(10);

            if (Input.GetKeyDown(KeyCode.V))
                // rend.material.color = Color.green;
                ChangColor(1);
        }

        [Command]
        public void ChangColor(int inter)
        {
            Yes(inter);
        }

        [ClientRpc]
        public void Yes(int inter)
        {
            ChnageColorLocal(inter);
        }

        public void ChnageColorLocal(int inter)
        {
            transform.localScale = Vector3.one * inter;
        }
    }
}