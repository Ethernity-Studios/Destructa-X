using System.Linq;
using Mirror;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Report : MonoBehaviour
{
    public TMP_Text OutgoingDamage;
    public TMP_Text IncomingDamage;
    public GameObject KilledYouText;
    public GameObject KillAssist;
    public TMP_Text KillAssistText;

    public Image TargetHead;
    public Image TargetBody;
    public Image TargetLegs;
    public TMP_Text TargetHeadHit;
    public TMP_Text TargetBodyHit;
    public TMP_Text TargetLegsHit;

    public Image OwnerHead;
    public Image OwnerBody;
    public Image OwnerLegs;
    public TMP_Text OwnerHeadHit;
    public TMP_Text OwnerBodyHit;
    public TMP_Text OwnerLegsHit;

    public Image AgentIcon;
    public TMP_Text PlayerName;
    public Image GunAbilityIcon;
    public GameObject DeathIcon;

    private GameManager gameManager;
    private GunManager gunManager;


    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        gunManager = FindObjectOfType<GunManager>();
    }

    public void UpdateReport(CombatReport report, Player targetPlayer)
    {
        OutgoingDamage.text = report.OutComingDamage.ToString();
        IncomingDamage.text = report.IncomingDamage.ToString();
        if(report.OwnerState == ReportState.Killed) KilledYouText.SetActive(true);
        
        //AgentIcon.sprite = report. TODO get agent sprite
        PlayerName.text = targetPlayer.PlayerName;
        //GunAbilityIcon.sprite = gunManager.GetGunByID(report.GunId).Icon;

        switch (report.TargetState)
        {
            case ReportState.Killed:
                KillAssist.SetActive(true);
                KillAssistText.text = "Killed";
                break;
        }
    }

    public void UpdateAssist()
    {
        KillAssist.SetActive(true);
        KillAssistText.text = "Assist";
    }
}
