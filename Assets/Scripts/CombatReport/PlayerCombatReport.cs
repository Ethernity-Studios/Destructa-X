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
                    rep.TargetState != ReportState.Killed && rep.TargetState != ReportState.Assisted)
                {
                    addNew = false;
                    
                    rep.GunId = report.GunId;
                    
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

                    rep.TargetState = report.TargetState;
                    rep.GunId = report.GunId;
                    
                    CmdGetTargetPlayer(rep.TargetPlayerId);
                    StartCoroutine(handleReport(CombatReports[index], rep, false, true));
                    if(rep.TargetState == ReportState.Killed) CmdNotifyPlayerDeath(rep.TargetPlayerId);
                    break;
                }

                addNew = true;
                index++;
            }
        }
        else addNew = true;

        if (!addNew) return;

        GameObject re = Instantiate(CombatReport, uiManager.CombatReport.transform);
        CombatReports.Add(re);
        CmdGetTargetPlayer(report.TargetPlayerId);
        StartCoroutine(handleReport(re, report, true, true));
    }

    [Command]
    void CmdNotifyPlayerDeath(uint targetPlayerId)
    {
        foreach (Player p in gameManager.PlayersID.Select(gameManager.GetPlayer))
        {
            p.GetComponent<PlayerCombatReport>().RpcNotifyPlayerDeath(targetPlayerId);
        }
    }

    [ClientRpc]
    void RpcNotifyPlayerDeath(uint targetPlayerId)
    {
        if (!isLocalPlayer) return;
        int index = 0;
        foreach (var rep in Reports)
        {
            if (rep.TargetPlayerId == targetPlayerId && rep.OwnerPlayerId != targetPlayerId)
            {
                CombatReports[index].GetComponent<Report>().UpdateAssist();
            }
            index++;
        }
    }
    

    IEnumerator handleReport(GameObject report, CombatReport rep, bool add, bool updateTarget)
    {
        while (targetPlayer == null) yield return null;
            report.GetComponent<Report>().UpdateReport(rep, targetPlayer);
            if(add) Reports.Add(rep);
        if(updateTarget) CmdUpdateTargetReport(rep);
        
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
        foreach (GameObject cr in CombatReports)
        {
            Destroy(cr);
        }
        CombatReports.Clear();
        Reports.Clear();  
    }


    [Command(requiresAuthority = false)]
    public void CmdHandleEnemyReport(CombatReport report)
    {
        foreach (Player p in gameManager.PlayersID.Select(gameManager.GetPlayer))
        {
            if (report.TargetPlayerId != p.netId) continue;
            RpcHandleEnemyReport(p.netIdentity.connectionToClient, report);
        }
        
    }

    [TargetRpc]
    public void RpcHandleEnemyReport(NetworkConnection conn,  CombatReport report)
    {
        int index = 0;
        bool addNew = false;
        if (Reports.Count > 0)
        {
            foreach (CombatReport combatReport in Reports)
            {
                if (report.TargetPlayerId == combatReport.OwnerPlayerId && report.OwnerPlayerId == combatReport.TargetPlayerId)
                {
                    addNew = false;
                    combatReport.GunId = report.GunId;

                    combatReport.IncomingDamage = report.OutComingDamage;
                    //combatReport.OutComingDamage = report.IncomingDamage;

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

                    combatReport.OwnerState = report.TargetState;
                    combatReport.GunId = report.GunId;
                    
                    CmdGetTargetPlayer(report.TargetPlayerId);
                    StartCoroutine(handleReport(CombatReports[index], combatReport, false, false));
                    break;
                }

                addNew = true;
                index++;
                
            }
        }
        else addNew = true;

        if (!addNew) return;
        CombatReport rep = createEnemyReport(report);
        GameObject re = Instantiate(CombatReport, uiManager.CombatReport.transform);
        CombatReports.Add(re);
        CmdGetTargetPlayer(report.TargetPlayerId);
        StartCoroutine(handleReport(re, rep, true, false));
    }

    CombatReport createEnemyReport(CombatReport report)
    {
        CombatReport rep = new()
        {
            GunId = report.GunId,
            OwnerPlayerId = report.TargetPlayerId,
            TargetPlayerId = report.OwnerPlayerId,
            IncomingDamage = report.OutComingDamage
        };

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

        rep.OwnerState = report.TargetState;
        rep.GunId = report.GunId;

        return rep;
    }
}