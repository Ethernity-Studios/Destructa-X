using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class ShopPlayer : MonoBehaviour
{
    [Header("Player")] 
    public int PlayerId;
    public Image AgentIcon;
    public TMP_Text Money;
    public TMP_Text Name;

    [Header("Inventory")]
    public GameObject Inventory;
    public Image ShieldIcon;
    public Image PrimaryGunIcon;
    public Image SecondaryGunIcon;
    
    [Header("Request")]
    public GameObject Request;
    public Image RequestedGunIcon;
}
