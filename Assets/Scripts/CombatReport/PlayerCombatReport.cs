using System.Collections.Generic;
using UnityEngine;
using Mirror;
using player;

public class PlayerCombatReport : NetworkBehaviour
{
    public readonly SyncList<CombatReport> Reports = new();

    [SerializeField] private GameObject CombatReport;
    public List<GameObject> CombatReports;

    private UIManager uiManager;

    private void Start()
    {
        GetComponent<PlayerUI>();
        uiManager = FindObjectOfType<UIManager>();
        FindObjectOfType<GameManager>();
    }

    [Command(requiresAuthority = false)]
    public void CmdAddReport(CombatReport report)
    {
        if (Reports.Count > 0)
        {
            int index = 0;
            foreach (CombatReport rep in Reports)
            {
                if (rep.OwnerPlayerId == report.OwnerPlayerId && rep.TargetPlayerId == report.TargetPlayerId &&
                    rep.TargetState != ReportState.Killed)
                {
                    Debug.Log("LOCAL - Updating existing report");

                    rep.GunId = report.GunId;

                    rep.IncomingDamage += report.IncomingDamage;
                    rep.OutComingDamage += report.OutComingDamage;
                    
                    if(report.OwnerBody.Count>0)
                        foreach (Body body in report.OwnerBody)
                        {
                            rep.OwnerBody.Add(body);
                        }
                    if(report.TargetBody.Count>0)
                        foreach (Body body in report.TargetBody)
                        {
                            rep.TargetBody.Add(body);
                        }
                    CombatReports[index].GetComponent<Report>().UpdateReport(report,NetworkServer.spawned[report.TargetPlayerId].GetComponent<Player>());
                    index++;
                }
                else
                {
                    Debug.Log("LOCAL - Adding new report :)");

                    GameObject r = Instantiate(CombatReport, uiManager.CombatReport.transform);
                    CombatReports.Add(r);
                    r.GetComponent<Report>().UpdateReport(report,NetworkServer.spawned[report.TargetPlayerId].GetComponent<Player>());
                    Reports.Add(report);
                }
            }
        }
        else
        {
            Debug.Log("LOCAL - Adding new report");

            GameObject r = Instantiate(CombatReport, uiManager.CombatReport.transform);
            CombatReports.Add(r);
            r.GetComponent<Report>().UpdateReport(report,NetworkServer.spawned[report.TargetPlayerId].GetComponent<Player>());
            Reports.Add(report);
        }
        
    }

    [ClientRpc]
    public void RpcClearReports()
    {
        Reports.Clear();
        foreach (GameObject cr in CombatReports)
        {
            Destroy(cr);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateReport(CombatReport report) => RpcUpdateReport(report);

    [ClientRpc]
    public void RpcUpdateReport(CombatReport report)
    {
        Debug.Log("Updating reports");

        foreach (CombatReport combatReport in Reports)
        {
            if (combatReport.TargetPlayerId == report.TargetPlayerId &&
                combatReport.OwnerPlayerId == report.OwnerPlayerId)
            {
                Debug.Log("Report exists - updating");
            }
            else
            {
                Debug.Log("Report does not exist");
            }
        }
    }
}