using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreboard : MonoBehaviour
{
    public TMP_Text Name;
    public TMP_Text Ultimate;
    public Image AgentIcon;
    public TMP_Text KDA;
    public Image GunIcon;
    public TMP_Text Money;
    public TMP_Text Ping;
    public Image Border;
    public Image Background;

    public Player Owner;

    private void Update()
    {
        if (Owner != null)
            Ping.text = Math.Round(Owner.Ping, 2) + " ms";
    }
}