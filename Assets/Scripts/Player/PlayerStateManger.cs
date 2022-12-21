using Mirror;
using UnityEngine;

namespace player
{
    public class PlayerStateManger : NetworkBehaviour
    {
        [SerializeField] UIManager uiManager;

        [ClientRpc]
        public void RpcSetupGame()
        {
            // loadingScreen.SetActive(false);
        }

        [ClientRpc]
        public void setPlayerColor(Player player, Color color)
        {
            player.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = color;
        }

        [ClientRpc]
        public void RpcFuckOfBoom()
        {
            /*
            var bombManager = Bomb.GetComponent<BombManager>();
            bombManager.canBoom = false;
            bombManager.StopAllCoroutines();
            bombManager.noBoomPwease();
            */
        }

        [ClientRpc]
        public void RpcSetDefaultPlayerSettings(Player player)
        {
            PlayerShootingManager playerShootingManager = player.GetComponent<PlayerShootingManager>();
            playerShootingManager.CanShoot = true;
            playerShootingManager.Reloading = false;

            PlayerBombManager playerBombmanager = player.GetComponent<PlayerBombManager>();
        }

        [ClientRpc]
        public void RpcGiveAmmo(Player player)
        {
            PlayerInventoryManager playerInventoryManager = player.GetComponent<PlayerInventoryManager>();
            if (playerInventoryManager.PrimaryGun != null)
            {
                GunInstance gunInstance = playerInventoryManager.PrimaryGunInstance.GetComponent<GunInstance>();
                // FIXME
                if (gunInstance == null)
                {
                    playerInventoryManager.PrimaryGun = null;
                }
                else
                {
                    gunInstance.Ammo = gunInstance.Gun.Ammo;
                    gunInstance.Magazine = gunInstance.Gun.MagazineAmmo;
                }
            }

            if (playerInventoryManager.SecondaryGun != null)
            {
                GunInstance gunInstance = playerInventoryManager.SecondaryGunInstance.GetComponent<GunInstance>();
                if (gunInstance == null)
                {
                    playerInventoryManager.SecondaryGun = null;
                }
                else
                {
                    gunInstance.Ammo = gunInstance.Gun.Ammo;
                    gunInstance.Magazine = gunInstance.Gun.MagazineAmmo;
                }
            }
        }

        [ClientRpc]
        public void RpcToggleMOTD(bool statement)
        {
            uiManager.MOTD.SetActive(statement);
            /*
            MOTD.SetActive(statement);
            */
        }
    }
}