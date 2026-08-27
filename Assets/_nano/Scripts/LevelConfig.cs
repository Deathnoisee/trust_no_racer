using System.Collections.Generic;
using UnityEngine;

// One asset per level/track. Right-click in the Project window ->
// Create -> Race -> Level Config to make one (e.g. "Level1", "Level2").
[CreateAssetMenu(fileName = "LevelConfig", menuName = "Race/Level Config")]
public class LevelConfig : ScriptableObject
{

    public string trackName;
    public int totalRacers = 8;


    [Min(1)] public int lapCount = 1;


    public List<CheatConfig> cheaterCheats = new List<CheatConfig>();
}