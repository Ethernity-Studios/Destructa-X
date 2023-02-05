using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    public TMP_Text MagazineText;
    public TMP_Text MaxAmmoText;

    public TMP_Text Health;
    public TMP_Text Shield;

    public TMP_Text Money;

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
