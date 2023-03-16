using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerCombatReport : NetworkBehaviour
{
    public SyncList<CombatReport> Reports;

    public void AddReport(CombatReport report)
    {
        int i = 0;
        foreach (CombatReport rep in Reports)
        {
            if (rep.Target == report.Target)
            {
                Reports[i].Gun = report.Gun;
                Reports[i].GunType = report.GunType;

                Reports[i].IncomingDamage = report.IncomingDamage;
                Reports[i].OutComingDamage = report.OutComingDamage;

                foreach (Body body in report.OwnerBody)
                {
                    Reports[i].OwnerBody.Add(body);
                }
                
                foreach (Body body in report.TargetBody)
                {
                    Reports[i].TargetBody.Add(body);
                }
                
            }
            i++;
        }
    }
}
