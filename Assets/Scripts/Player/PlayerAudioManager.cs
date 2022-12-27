using Mirror;
using UnityEngine;

namespace player
{
    public class PlayerAudioManager : NetworkBehaviour
    {
        public AudioSource Source;

        private void Start()
        {
            if (!isLocalPlayer) return;

            Source = gameObject.GetComponent<AudioSource>();
        }

        [Command]
        public void PlaySomething()
        {
        }


        [ClientRpc]
        public void PlayForClients()
        {
        }
    }
}