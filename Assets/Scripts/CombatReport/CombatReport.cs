using System.Collections.Generic;

public enum Body{
    None, Head, Body, Legs
}

public enum ReportState
{
    Alive, Killed, Assisted
}
[System.Serializable]
public class CombatReport
{
    public uint TargetPlayerId;
    public uint OwnerPlayerId;
    
    public int OutComingDamage;
    public int IncomingDamage;

    public int GunId;

    public List<Body> TargetBody = new();
    public List<Body> OwnerBody = new();
    
    public ReportState TargetState = ReportState.Alive;
    public ReportState OwnerState = ReportState.Alive;
}
