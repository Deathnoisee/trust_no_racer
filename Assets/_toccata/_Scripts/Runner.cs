using UnityEngine;
using System.Collections.Generic;
public class Runner
{
    [Header("Visuals")]
    public Sprite hair;
    public Sprite eyes;
    public Sprite mouth;
    public Sprite shoes;

    public Color shirtColor;
    public Color skinColor;
    public Color shoesColor;
    public Color hairColor;

    [Header("Cheats")]
    public CheatType cheatType;
    public bool cheatActivated = false;

    //[Header("Stats")]
    //public float avgSpeed;
    //public float speedVariance;


    [Header("GeneralInfo")]
    public string runnerName;
    public float weight;
    public float height;
    public int age;
    public int runnerID;


    public List<string> runningHistory;

}
