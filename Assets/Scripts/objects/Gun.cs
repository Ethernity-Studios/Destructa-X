using UnityEngine;

[CreateAssetMenu]
public class Gun : ScriptableObject
{
    public string Name;
    public int MagazineSize;
    public int MaxAmmo;
    public int ReloadTime;

    public Fire LMB;
    public RMB RMB;
}