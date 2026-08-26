using System.Collections.Generic;
using UnityEngine;

// One asset per level/track. Right-click in the Project window ->
// Create -> Race -> Level Config to make one (e.g. "Level1", "Level2").
[CreateAssetMenu(fileName = "LevelConfig", menuName = "Race/Level Config")]
public class LevelConfig : ScriptableObject
{
    public int totalRacers = 8;

    [Tooltip("One entry = one cheater with exactly that one cheat. The number of cheaters in " +
             "this level is simply the length of this list — e.g. 2 entries here means 2 cheaters, " +
             "each doing whichever cheat you gave them. Leave it empty for a clean level with no cheaters.\n\n" +
             "Example — Level 1: two ShortcutCut entries (both cheaters cut).\n" +
             "Example — Level 2: one ShortcutCut entry + one SpeedBoost entry (one cutter, one on the drugs).")]
    public List<CheatConfig> cheaterCheats = new List<CheatConfig>();
}