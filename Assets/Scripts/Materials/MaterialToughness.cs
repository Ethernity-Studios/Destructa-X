using UnityEngine;

public enum Toughness
{
    ExtraSoft, Soft, MediumSoft, Medium, MediumHard, Hard, ExtraHard
}
public class MaterialToughness : MonoBehaviour
{
    public Toughness Toughness;
    private void Awake()
    {
        ToughnessAmount = Toughness switch
        {
            Toughness.ExtraSoft => 3,
            Toughness.Soft => 4,
            Toughness.Medium => 5,
            Toughness.MediumSoft => 6,
            Toughness.MediumHard => 7,
            Toughness.Hard => 8,
            Toughness.ExtraHard => 9,
            _ => ToughnessAmount
        };
    }
    public int ToughnessAmount;
}