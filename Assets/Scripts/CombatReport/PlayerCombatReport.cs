using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerCombatReport : NetworkBehaviour
{
    public readonly SyncList<CombatReport> Reports = new();

    [SerializeField] private GameObject CombatReport;
    public List<GameObject> CombatReports;

    private UIManager uiManager;

    [SerializeField]  private Player targetPlayer;
    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
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
                    addNew = false;
                    
                    rep.GunId = report.GunId;

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

                    StartCoroutine(handleReport(CombatReports[index], rep));
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
        StartCoroutine(handleReport(re, report));
    }

    IEnumerator handleReport(GameObject report, CombatReport rep)
    {
        CmdGetTargetPlayer(rep.TargetPlayerId);
        while (targetPlayer == null) yield return null;
            report.GetComponent<Report>().UpdateReport(rep, targetPlayer);
        CmdAddReport(rep);
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
    public void CmdHandleEnemyReport(CombatReport report) => RpcHandleEnemyReport(report);

    [ClientRpc]
    public void RpcHandleEnemyReport(CombatReport report)
    {
        Debug.Log("Handling enemy report!!");
        if (Reports.Count > 0)
        {
            foreach (CombatReport combatReport in Reports)
            {
                if (combatReport.TargetPlayerId == report.TargetPlayerId && combatReport.OwnerPlayerId == report.OwnerPlayerId)
                {
                    Debug.Log("Report exists - updating");
                }
                else
                {
                    Debug.Log("Report does not exist");
                }
            }
        }
        else
        {
            Debug.Log("No reports found, creating new");
            report.GunId = report.GunId;

            report.IncomingDamage += report.IncomingDamage;
            report.OutComingDamage += report.OutComingDamage;

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
            GameObject re = Instantiate(CombatReport, uiManager.CombatReport.transform);
            CombatReports.Add(re);
            StartCoroutine(handleReport(re, report));
        }
    }
}