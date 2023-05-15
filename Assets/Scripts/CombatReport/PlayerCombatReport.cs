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
    private GunManager gunManager;

    [SerializeField] private Player targetPlayer;

    [SerializeField] private GameObject killFeed;

    private Player Player;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        gameManager = FindObjectOfType<GameManager>();
        gunManager = FindObjectOfType<GunManager>();
        Player = GetComponent<Player>();
    }

    public void AddReport(CombatReport report, bool penetrated, bool headshot, int kills)
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
                    if (rep.TargetState == ReportState.Killed)
                    {
                        CmdNotifyPlayerDeath(rep.TargetPlayerId, rep.OwnerPlayerId);
                        CmdSpawnKillFeed(rep, penetrated, headshot, kills);
                    }

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
    void CmdSpawnKillFeed(CombatReport report, bool penetrated, bool headshot, int kills)
    {
        GameObject killFeed = Instantiate(this.killFeed);
        NetworkServer.Spawn(killFeed);

        Player sourcePlayer = NetworkServer.spawned[report.OwnerPlayerId].GetComponent<Player>();
        Player targetPlayer = NetworkServer.spawned[report.TargetPlayerId].GetComponent<Player>();

        Player.CmdAddKillThisRound();

        foreach (Player p in gameManager.PlayersID.Select(gameManager.GetPlayer))
        {
            if (p.PlayerTeam == Player.PlayerTeam) RpcSpawnTeamKillFeed(p.netIdentity.connectionToClient, report, killFeed, Player, targetPlayer, penetrated, headshot, kills);
            else RpcSpawnEnemyKillFeed(p.netIdentity.connectionToClient, report, killFeed, Player, targetPlayer, penetrated, headshot, kills);
        }

        Invoke(nameof(UnSpawnKillFeed), 60f);
    }

    [TargetRpc]
    void RpcSpawnTeamKillFeed(NetworkConnection conn, CombatReport report, GameObject killFeed, Player sourcePlayer, Player targetPlayer, bool penetrated, bool headshot, int kills)
    {
        KillFeed kf = killFeed.GetComponent<KillFeed>();
        killFeed.transform.SetParent(uiManager.KillFeed.transform);
        killFeed.transform.localScale = Vector3.one;

        kf.KillStreak.text = kills switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            6 => "VI",
            7 => "VII",
            _ => kf.KillStreak.text
        };
        //kf.SourcePlayerAgent.sprite = TODO
        kf.SourcePlayerName.text = sourcePlayer.PlayerName;
        kf.Gun.sprite = gunManager.GetGunByID(report.GunId).Icon;
        kf.Penetration.SetActive(penetrated);
        kf.Headshot.SetActive(headshot);
        kf.TargetPlayerName.text = targetPlayer.PlayerName;
        kf.TargetPlayerBackground.sprite = uiManager.EnemyTeamAgentBorder;
        //kf.TargetPlayerAgent.sprite = TODO
    }

    [TargetRpc]
    void RpcSpawnEnemyKillFeed(NetworkConnection conn, CombatReport report, GameObject killFeed, Player sourcePlayer, Player targetPlayer, bool penetrated, bool headshot, int kills)
    {
        KillFeed kf = killFeed.GetComponent<KillFeed>();
        killFeed.transform.SetParent(uiManager.KillFeed.transform);
        killFeed.transform.localScale = Vector3.one;

        kf.Background.sprite = uiManager.RedTeamBackgroundScoreboard;
        kf.KillStreakBackground.sprite = uiManager.EnemyTeamKillStreakBackground;
        kf.KillStreak.text = kills switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            6 => "VI",
            7 => "VII",
            _ => kf.KillStreak.text
        };
        kf.SourcePlayerAgentBackground.sprite = uiManager.EnemyTeamAgentBorder;
        //kf.SourcePlayerAgent.sprite = TODO
        kf.SourcePlayerName.text = sourcePlayer.PlayerName;
        kf.Gun.sprite = gunManager.GetGunByID(report.GunId).Icon;
        kf.Penetration.SetActive(penetrated);
        kf.Headshot.SetActive(headshot);
        kf.TargetPlayerName.text = targetPlayer.PlayerName;
        kf.TargetPlayerBackground.sprite = uiManager.FriendlyTeamAgentBorder;
        //kf.TargetPlayerAgent.sprite = TODO
    }


    [Server]
    void UnSpawnKillFeed()
    {
        Debug.Log("UnSpawning" + uiManager.KillFeed.transform.GetChild(0).gameObject);
        NetworkServer.UnSpawn(uiManager.KillFeed.transform.GetChild(0).gameObject);
        Destroy(uiManager.KillFeed.transform.GetChild(0).gameObject);
    }


    [Command]
    void CmdNotifyPlayerDeath(uint targetPlayerId, uint ownerPlayerId)
    {
        foreach (Player p in gameManager.PlayersID.Select(gameManager.GetPlayer))
        {
            p.GetComponent<PlayerCombatReport>().RpcNotifyPlayerDeath(targetPlayerId, ownerPlayerId);
        }
    }

    [ClientRpc]
    void RpcNotifyPlayerDeath(uint targetPlayerId, uint ownerPlayerId)
    {
        if (!isLocalPlayer) return;
        int index = 0;
        foreach (var rep in Reports)
        {
            if (rep.TargetPlayerId == targetPlayerId && rep.OwnerPlayerId != ownerPlayerId)
            {
                Debug.Log("owner player id" + ownerPlayerId);
                Debug.Log("rep target id" + rep.TargetPlayerId);
                Debug.Log("rep owner id" + rep.OwnerPlayerId);
                Debug.Log("target player id" + targetPlayerId);


                CombatReports[index].GetComponent<Report>().UpdateAssist();
                GetComponent<Player>().CmdAddAssist();
                Debug.Log("Toggling assist");
            }

            index++;
        }
    }

    IEnumerator handleReport(GameObject report, CombatReport rep, bool add, bool updateTarget)
    {
        while (targetPlayer == null) yield return null;
        report.GetComponent<Report>().UpdateReport(rep, targetPlayer);
        if (add) Reports.Add(rep);
        if (updateTarget) CmdUpdateTargetReport(rep);

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
    public void RpcHandleEnemyReport(NetworkConnection conn, CombatReport report)
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