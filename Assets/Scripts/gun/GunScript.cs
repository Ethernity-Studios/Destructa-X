using UnityEngine;

namespace gun
{
    public class GunScript : MonoBehaviour
    {
        public GunInstance Gun;

        public void Init(GunInstance gun)
        {
            Gun = gun;
        }
    }
}