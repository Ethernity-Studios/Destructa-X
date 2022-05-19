using UnityEngine;

public enum Toughness
{
    ExtraSoft, Soft, MediumSoft, Medium, MediumHard, Hard, ExtraHard
}
public class MaterialToughness : MonoBehaviour
{
    private void Awake()
    {
        if (Toughness == Toughness.ExtraSoft) ToughnessAmount = 1;
        else if (Toughness == Toughness.Soft) ToughnessAmount = 2;
        else if (Toughness == Toughness.Medium) ToughnessAmount = 3;
        else if (Toughness == Toughness.MediumSoft) ToughnessAmount = 4;
        else if (Toughness == Toughness.MediumHard) ToughnessAmount = 5;
        else if (Toughness == Toughness.Hard) ToughnessAmount = 6;
        else if (Toughness == Toughness.ExtraHard) ToughnessAmount = 7;
    }
    public int ToughnessAmount;
    public Toughness Toughness;
}