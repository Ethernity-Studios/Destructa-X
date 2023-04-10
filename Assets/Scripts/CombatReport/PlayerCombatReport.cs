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
                    StartCoroutine(handleReport(CombatReports[index], rep, false, true));
                    break;
                }

                if (report.OwnerPlayerId == rep.TargetPlayerId && report.TargetPlayerId == rep.OwnerPlayerId)
                {
                    Debug.Log("Updating existing report 2");
                    addNew = false;
                    
                    rep.OwnerGunId = report.OwnerGunId;

                    //rep.IncomingDamage += report.IncomingDamage;
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
                    StartCoroutine(handleReport(CombatReports[index], rep, false, true));
                    break;
                }

                addNew = true;
                index++;
            }
        }
        else addNew = true;

        if (!addNew) return;
        Debug.Log($"Creating new report with owner: {report.OwnerPlayerId} target: {report.TargetPlayerId}");
        GameObject re = Instantiate(CombatReport, uiManager.CombatReport.transform);
        CombatReports.Add(re);
        CmdGetTargetPlayer(report.TargetPlayerId);
        StartCoroutine(handleReport(re, report, true, true));
    }

    IEnumerator handleReport(GameObject report, CombatReport rep, bool add, bool update)
    {
        while (targetPlayer == null) yield return null;
            report.GetComponent<Report>().UpdateReport(rep, targetPlayer);
            if(add) Reports.Add(rep);
        if(update) CmdUpdateTargetReport(rep);
        
        targetPlayer = null;
    }

    [Command(requiresAuthority = false)]
    void CmdUpdateTargetReport(CombatReport report) => NetworkServer.spawned[report.TargetPlayerId].GetComponent<PlayerCombatReport>().CmdHandleEnemyReport(report);

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
            p.GetComponent<PlayerCombatReport>();
            if (report.TargetPlayerId == p.netId)
            {
                Debug.Log("Handling enemy report on player with id: " + p.netId);
                RpcHandleEnemyReport(p.netIdentity.connectionToClient, report);
                return;
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
        Debug.Log($"Handling enemy report!! owner: {report.OwnerPlayerId} target: {report.TargetPlayerId}");
        int index = 0;
        bool addNew = false;
        if (Reports.Count > 0)
        {
            foreach (CombatReport combatReport in Reports)
            {
                if (combatReport.TargetPlayerId == report.OwnerPlayerId && combatReport.OwnerPlayerId == report.TargetPlayerId)
                {
                    addNew = false;
                    Debug.Log("Report exists - updating 1");
                    combatReport.OwnerGunId = report.OwnerGunId;

                    combatReport.IncomingDamage = report.OutComingDamage;
                    combatReport.OutComingDamage = report.IncomingDamage;

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
                    StartCoroutine(handleReport(CombatReports[index], combatReport, false, false));
                    break;
                }
                if (combatReport.TargetPlayerId == report.TargetPlayerId && combatReport.OwnerPlayerId == report.OwnerPlayerId)
                {
                    addNew = false;
                    Debug.Log("Report exists - updating 2");
                    combatReport.OwnerGunId = report.OwnerGunId;

                    combatReport.IncomingDamage = report.OutComingDamage;
                    combatReport.OutComingDamage = report.IncomingDamage;

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
                    StartCoroutine(handleReport(CombatReports[index], combatReport, false, false));
                    break;
                }
                
                addNew = true;
                index++;
                
            }
        }
        else
        {
            addNew = true;
        }
        
        if (!addNew) return;
        Debug.Log($"Report does not exist, creating new owner: {report.OwnerPlayerId} target: {report.TargetPlayerId}");
        CombatReport rep = createEnemyReport(report);
        GameObject re = Instantiate(CombatReport, uiManager.CombatReport.transform);
        CombatReports.Add(re);
        CmdGetTargetPlayer(report.TargetPlayerId);
        StartCoroutine(handleReport(re, rep, true, false));
    }

    CombatReport createEnemyReport(CombatReport report)
    {
        CombatReport rep = new();
        
        Debug.Log("Creating enemy report");
        rep.OwnerGunId = report.OwnerGunId;

        rep.OwnerPlayerId = report.OwnerPlayerId;
        rep.TargetPlayerId = report.TargetPlayerId;

        rep.IncomingDamage = report.OutComingDamage;
        //rep.OutComingDamage += report.IncomingDamage;

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

        return rep;
    }
}