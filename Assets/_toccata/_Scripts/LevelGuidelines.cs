using UnityEngine;

[CreateAssetMenu(fileName = "LevelGuidelines", menuName = "Scriptable Objects/LevelGuidelines")]
public class LevelGuidelines : ScriptableObject
{
    [Header("Banned things statements")]
    public string[] bannedStuff;


    [Header("The banned items")]
    public Nationality[] bannedNationalities;
    public Sprite[] bannedShoeSprites;
    public Sprite[] bannedHairSprites;

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