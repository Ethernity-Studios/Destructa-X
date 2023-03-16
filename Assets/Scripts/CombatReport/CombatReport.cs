using System.Collections.Generic;

public enum Body{
    Head, Body, Legs
}

public class CombatReport
{
    public Player Target;
    public Player Owner;
    
    public int OutComingDamage;
    public int IncomingDamage;

    public Gun Gun;
    public GunType GunType;

    public List<Body> TargetBody;
    public List<Body> OwnerBody;

}
