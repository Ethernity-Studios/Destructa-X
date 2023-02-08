using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static Color GreenColor = new Color(142/255f, 214/255f, 161/255f, 1);
    public static Color RedColor = new Color(214/255f, 143/255f, 142/255f, 1);
    public static Color LightGreyColor = new Color(200/255f, 200/255f, 200/255f, 1);
    
    [Header("Player")]
    public TMP_Text MagazineText;
    public TMP_Text MaxAmmoText;
    public GameObject BulletsIcon;
    public Image EquippedGun;
    public Image PreviousEquippedGun;
    public Image PrePreviousEquippedGun;
[Space(25)]
    public TMP_Text Health;
    public Image HealthBar;
    public TMP_Text Shield;
    public Image ShieldBar;
    public TMP_Text Money;
    public Image AgentIcon;
[Space(25)]
    public TMP_Text UltimateBindKey;
    public Image UltimateIcon;
    [SerializeField] private GameObject UltimatePoint;
    private List<Image> UltimatePoints;

    [Header("Spectating")]
    public GameObject SpectatingUI;
    public TMP_Text SpectatingPlayerName;

    [Header("Game State")]
    public GameObject GameState;
    public TMP_Text GameStateText;
    public TMP_Text GameStateSubText;

    public string BuyPhaseText, BuyPhaseSubText;
    
    public string RoundWinText, RoundWinSubText;
    public string RoundLoseText, RoundLoseSubText;

}
