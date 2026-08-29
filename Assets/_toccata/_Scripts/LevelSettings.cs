using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSettings", menuName = "Scriptable Objects/LevelSettings")]
public class LevelSettings : ScriptableObject
{
    public List<cheatAmount> runnerSettings;
    public LevelGuidelines guideLines;

    public int drugTests = 3;
    public int trajectoryTests = 3;
    public int varCheck = 0;
    public int lieDetection = 0;


}

[System.Serializable]
public struct cheatAmount
{
    public int amount;
    public CheatType cheatType;
}