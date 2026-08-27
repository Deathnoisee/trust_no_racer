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
    public Cheatos cheatType;
    public bool cheatActivated = false;

    //[Header("Stats")]
    //public float avgSpeed;
    //public float speedVariance;


    [Header("GeneralInfo")]
    public string runnerName;
    public float weight;
    public float height;
    public int age;
    public Gender gender;
    public int runnerID;
    public Nationality runnerNationality;

    public List<string> runningHistory;


    public bool isFakePersona;
    public RunnerData fakePersona;

    public bool isExtra;

}




public enum Nationality
{
    Galean,
    Pyronian,
    Veldtish,
    Stratusian,
    Zeyphran,
    Miragian
}

public enum Gender
{
    Male,
    Female,
}