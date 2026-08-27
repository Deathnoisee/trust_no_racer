using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSettings", menuName = "Scriptable Objects/LevelSettings")]
public class LevelSettings : ScriptableObject
{
    public List<cheatAmount> runnerSettings;
    public LevelGuidelines guideLines;
}

[System.Serializable]
public struct cheatAmount
{
    public int amount;
    public CheatType cheatType;
}