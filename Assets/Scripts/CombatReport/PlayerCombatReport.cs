using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class PlayerCombatReport : NetworkBehaviour
{
    public List<CombatReport> Reports = new();

    [SerializeField] private GameObject CombatReport;
    public List<GameObject> CombatReports;

    private UIManager uiManager;
    private GameManager gameManager;

    [SerializeField]  private Player targetPlayer;
    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        gameManager = FindObjectOfType<GameManager>();
        FindObjectOfType<GameManager>();
        FindObjectOfType<GameManager>();
    }
    
    public void AddReport(CombatReport report)
    {
        if (!isLocalPlayer) return;
        bool addNew = false;
        int index = 0;
        if (CombatReports.Count > 0)
        {
            foreach (CombatReport rep in Reports)
            {
                if (rep.OwnerPlayerId == report.OwnerPlayerId && rep.TargetPlayerId == report.TargetPlayerId &&
                    rep.TargetState != ReportState.Killed)
                {
                    Debug.Log("Updating existing report");
                    addNew = false;
                    
                    rep.OwnerGunId = report.OwnerGunId;

                    rep.IncomingDamage += report.IncomingDamage;
                    rep.OutComingDamage += report.OutComingDamage;

                    if (report.OwnerBody.Count > 0)
                        foreach (Body body in report.OwnerBody)
                        {
                            rep.OwnerBody.Add(body);
                        }

                    if (report.TargetBody.Count > 0)
                        foreach (Body body in report.TargetBody)
                        {
                            rep.TargetBody.Add(body);
                        }
                    CmdGetTargetPlayer(rep.TargetPlayerId);
                    StartCoroutine(handleReport(CombatReports[index], rep, false));
                    break;
                }

                addNew = true;
                index++;
            }
        }
        else addNew = true;

        if (!addNew) return;
        Debug.Log("Creating new report");
        GameObject re = Instantiate(CombatReport, uiManager.CombatReport.transform);
        CombatReports.Add(re);
        CmdGetTargetPlayer(report.TargetPlayerId);
        StartCoroutine(handleReport(re, report, true));
    }

    IEnumerator handleReport(GameObject report, CombatReport rep, bool add)
    {
        while (targetPlayer == null) yield return null;
            report.GetComponent<Report>().UpdateReport(rep, targetPlayer);
        //CmdAddReport(rep);
        if(add) Reports.Add(rep);
        CmdUpdateTargetReport(rep);
        targetPlayer = null;
    }

    [Command]
    void CmdUpdateTargetReport(CombatReport report) => NetworkServer.spawned[report.TargetPlayerId].GetComponent<PlayerCombatReport>().CmdHandleEnemyReport(report);

    [Command]
    void CmdAddReport(CombatReport report) => Reports.Add(report);

    [Command(requiresAuthority = false)]
    void CmdGetTargetPlayer(uint playerId) => RpcGetTargetPlayer(NetworkServer.spawned[playerId].GetComponent<Player>());

    [ClientRpc]
    void RpcGetTargetPlayer(Player player)
    {
        if (!isLocalPlayer) return;
        targetPlayer = player;
    }

    [ClientRpc]
    public void RpcClearReports()
    {
        if (!isLocalPlayer) return;
        CmdClearReports();
        foreach (GameObject cr in CombatReports)
        {
            Destroy(cr);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdClearReports() => Reports.Clear();


    [Command(requiresAuthority = false)]
    public void CmdHandleEnemyReport(CombatReport report)
    {
        foreach (Player p in gameManager.PlayersID.Select(gameManager.GetPlayer))
        {
            
            PlayerCombatReport playerCombatReport = p.GetComponent<PlayerCombatReport>();
            if (report.TargetPlayerId == p.netId && !isLocalPlayer)
            {
                Debug.Log("Handling enemy report on player with id: " + p.netId);
                RpcHandleEnemyReport(p.netIdentity.connectionToClient, report);
            }
            /*foreach (CombatReport unused in playerCombatReport.Reports.Where(cr => cr.TargetPlayerId == p.netId))
            {
                if (isLocalPlayer) return;
                Debug.Log("Handling enemy report on player with id: " + p.netId);
                RpcHandleEnemyReport(p.netIdentity.connectionToClient, report);
            }*/
        }
        
    }

    [TargetRpc]
    public void RpcHandleEnemyReport(NetworkConnection conn,  CombatReport report)
    {
        Debug.Log("Handling enemy report!!");
        int index = 0;
        if (Reports.Count > 0)
        {
            foreach (CombatReport combatReport in Reports)
            {
                if (combatReport.TargetPlayerId == report.OwnerPlayerId && combatReport.OwnerPlayerId == report.TargetPlayerId)
                {
                    Debug.Log("Report exists - updating");
                    combatReport.OwnerGunId = report.OwnerGunId;

                    combatReport.IncomingDamage += report.OutComingDamage;
                    combatReport.OutComingDamage += report.IncomingDamage;

                    if (report.OwnerBody.Count > 0)
                        foreach (Body body in report.OwnerBody)
                        {
                            combatReport.OwnerBody.Add(body);
                        }

                    if (report.TargetBody.Count > 0)
                        foreach (Body body in report.TargetBody)
                        {
                            combatReport.TargetBody.Add(body);
                        }
                    CmdGetTargetPlayer(report.TargetPlayerId);
                    StartCoroutine(handleReport(CombatReports[index], combatReport, false));
                }
                else
                {
                    Debug.Log("Report does not exist, creating new");
                    CombatReport rep = createEnemyReport(report);
                    GameObject re = Instantiate(CombatReport, uiManager.CombatReport.transform);
                    CombatReports.Add(re);
                    CmdGetTargetPlayer(report.TargetPlayerId);
                    StartCoroutine(handleReport(re, rep, true));
                    break;
                }

                index++;
            }
        }
        else
        {
            Debug.Log("No reports found, creating new");
            CombatReport rep = createEnemyReport(report);
            GameObject re = Instantiate(CombatReport, uiManager.CombatReport.transform);
            CombatReports.Add(re);
            CmdGetTargetPlayer(report.TargetPlayerId);
            StartCoroutine(handleReport(re, rep, true));
        }
    }

    CombatReport createEnemyReport(CombatReport report)
    {
        Debug.Log("Creating enemy report");
        report.OwnerGunId = report.OwnerGunId;

        report.IncomingDamage += report.OutComingDamage;
        report.OutComingDamage += report.IncomingDamage;

        if (report.OwnerBody.Count > 0)
            foreach (Body body in report.OwnerBody)
            {
                report.OwnerBody.Add(body);
            }

        if (report.TargetBody.Count > 0)
            foreach (Body body in report.TargetBody)
            {
                report.TargetBody.Add(body);
            }

        return report;
    }
}