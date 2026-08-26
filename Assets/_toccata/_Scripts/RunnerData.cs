using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpriteView
{
    public Sprite frontView;
    public Sprite sideView;
}


public class RunnerData
{
    [Header("Visuals")]
    public SpriteView hair;
    public SpriteView eyes;
    public SpriteView mouth;
    public SpriteView shoes;

    public Color hairColor;
    public Color shirtColor;
    public Color skinColor;
    public Color shoesColor;

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
    public Nationality runnerNationality;

    public List<string> runningHistory;

}
