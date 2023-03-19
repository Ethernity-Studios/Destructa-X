using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static Color GreenColor = new Color(142/255f, 214/255f, 161/255f, 1);
    public static Color RedColor = new Color(214/255f, 143/255f, 142/255f, 1); //#D68F8E
    public static Color LightGreyColor = new Color(200/255f, 200/255f, 200/255f, 1);
    public static Color YellowColor = new Color(214 / 255f, 214 / 255f, 142 / 255f, 1);
    
    [Header("Player")]
    public TMP_Text MagazineText;
    public TMP_Text MaxAmmoText;
    public GameObject BulletsIcon;
    public Image EquippedItem;
    public Image PreviousEquippedItem;
    [Space(25)]
    public TMP_Text Health;
    public Image HealthBar;
    public TMP_Text Shield;
    public Image ShieldBar;
    public TMP_Text Money;
    public Image AgentIcon;
    public GameObject Bomb;
[Space(25)]
    public TMP_Text UltimateBindKey;
    public Image UltimateIcon;
    [SerializeField] private GameObject UltimatePoint;
    public List<Image> UltimatePoints;

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

    [Header("Header")]
    public GameObject HeaderPlayer;
    public Sprite RedTeamBackgroundHeader;

    public GameObject FriendlyTeamHeader;
    public GameObject EnemyTeamHeader;

    [Header("Shop")] 
    public GameObject ShopTeam;
    
    public Image ShopPlayerAgentIcon;
    public Image ShopPlayerShieldIcon;
    public Image ShopPlayerPrimaryGun;
    public Image ShopPlayerSecondaryGun;
    public TMP_Text ShopPlayerMoney;
    public TMP_Text ShopPlayerMinMoneyNextRound;

    public GameObject ShopPlayer;

    [Header("Scoreboard")] 
    public GameObject Scoreboard;
    public GameObject PlayerScoreboard;

    public GameObject FriendlyTeamScoreboard;
    public GameObject EnemyTeamScoreboard;

    public Sprite RedTeamBackgroundScoreboard;

    public TMP_Text BlueScoreboardScore;
    public TMP_Text RedScoreboardScore;

    [Header("CombatReport")] 
    public GameObject CombatReport;

    [Header("Items")] 
    public Sprite Knife;

    [Header("Performance")] 
    public TMP_Text Latency;
    
    [Header("Misc")]
    public Sprite TransparentImage;
}
