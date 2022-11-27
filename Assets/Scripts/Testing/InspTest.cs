using UnityEditor;
using UnityEngine;
using System;

[CreateAssetMenu]
public class InspTest : ScriptableObject
{
    public string Name;

    public Insane Gej;
}

[Serializable]
public class Insane
{
    public float testttttt;
    public FireMode FireMode;
}