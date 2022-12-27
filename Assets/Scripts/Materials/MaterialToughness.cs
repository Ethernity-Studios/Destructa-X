using UnityEngine;

public enum Toughness
{
    ExtraSoft, Soft, MediumSoft, Medium, MediumHard, Hard, ExtraHard
}
public class MaterialToughness : MonoBehaviour
{
    private void Awake()
    {
        ToughnessAmount = Toughness switch
        {
            Toughness.ExtraSoft => 1,
            Toughness.Soft => 2,
            Toughness.Medium => 3,
            Toughness.MediumSoft => 4,
            Toughness.MediumHard => 5,
            Toughness.Hard => 6,
            Toughness.ExtraHard => 7,
            _ => ToughnessAmount
        };
    }
    public int ToughnessAmount;
    public Toughness Toughness;
}