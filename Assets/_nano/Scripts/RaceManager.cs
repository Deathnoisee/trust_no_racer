using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class RaceManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject runnerPrefab;
    public Transform startPoint;
    public SplineContainer mainSpline;

    [Header("Colors")]
    public Color[] runnerColors;

    [Header("Spawn Points")]

    public Transform[] spawnPoints;



    [Header("Road")]

    public float roadHalfWidth = 1.5f;

    [Header("Levels")]

    public List<LevelConfig> levels = new List<LevelConfig>();
    public int currentLevelIndex = 0;


    public int cheaterCount = 0;

    private bool raceStarted = false;
    private bool raceEnded = false;
    private List<Runner> racers = new List<Runner>();

    private LevelConfig CurrentLevel =>
        (levels != null && currentLevelIndex >= 0 && currentLevelIndex < levels.Count) ? levels[currentLevelIndex] : null;

    void Start()
    {
        LoadLevel(currentLevelIndex);
    }

    // Call this to (re)start the race manager on a given level — from a menu, a level-select
    // trigger, "next level" button, etc. Clears out the previous level's runners first, so the
    // same RaceManager instance can be reused for the whole game.
    public void LoadLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        raceStarted = false;
        raceEnded = false;

        foreach (Runner runner in racers)
        {
            if (runner != null) Destroy(runner.gameObject);
        }
        racers.Clear();

        SpawnRacers();
    }

    // Convenience for "next level" buttons/triggers. Does nothing (just logs) once
    // you're past the last configured level.
    public void LoadNextLevel()
    {
        int nextIndex = currentLevelIndex + 1;
        if (levels != null && nextIndex < levels.Count)
        {
            LoadLevel(nextIndex);
        }
        else
        {
            Debug.Log("No more levels configured — this was the last one.");
        }
    }

    void SpawnRacers()
    {
        LevelConfig level = CurrentLevel;
        int racerCount = level != null ? level.totalRacers : 8; // default to 8 if no level config is set
        List<CheatConfig> cheatsToAssign = level != null ? level.cheaterCheats : new List<CheatConfig>();

        // one entry in cheatsToAssign = one cheater with exactly that cheat, so pick that many
        // distinct racer slots up front and map each to its cheat.
        Dictionary<int, CheatConfig> cheatByRacerIndex = AssignCheatsToRacers(cheatsToAssign, racerCount);



        for (int i = 0; i < racerCount; i++)
        {
            Vector3 spawnPos = GetSpawnPosition(i);

            GameObject obj = Instantiate(runnerPrefab, spawnPos, Quaternion.identity);
            Runner runner = obj.GetComponent<Runner>();

            runner.runnerBibNumber = i;
            runner.runnerName = "Runner " + i;
            runner.runnerColor = runnerColors[i % runnerColors.Length];
            runner.mainSpline = mainSpline;
            runner.baseSpeed = Random.Range(3.5f, 4.5f);
            runner.roadHalfWidth = roadHalfWidth;
            runner.totalLaps = level != null ? level.lapCount : 1;

            if (cheatByRacerIndex.TryGetValue(i, out CheatConfig cheat))
            {
                runner.isCheater = true;
                runner.assignedCheat = cheat; // exactly one — never a list
            }

            // convert the designated spawn position into (t, laneOffset) so the runner's
            // very first Tick() continues smoothly from exactly where it was placed
            SplineKnotUtils.GetNearestTAndLateralOffset(mainSpline, spawnPos, out float spawnT, out float spawnLateral);
            runner.SetSpawn(spawnT, spawnLateral, spawnPos);

            racers.Add(runner);
        }
    }

    Dictionary<int, CheatConfig> AssignCheatsToRacers(List<CheatConfig> cheatsToAssign, int racerCount)
    {
        Dictionary<int, CheatConfig> result = new Dictionary<int, CheatConfig>();
        HashSet<int> usedIndices = new HashSet<int>();

        foreach (CheatConfig cheat in cheatsToAssign)
        {
            if (usedIndices.Count >= racerCount) break; // more cheats configured than racers available

            int index;
            do { index = Random.Range(0, racerCount); } while (usedIndices.Contains(index));

            usedIndices.Add(index);
            result[index] = cheat;
        }

        return result;
    }

    Vector3 GetSpawnPosition(int index)
    {
        if (spawnPoints != null && index < spawnPoints.Length && spawnPoints[index] != null)
        {
            return spawnPoints[index].position;
        }

        return spawnPoints == null || spawnPoints.Length == 0 ? startPoint.position : startPoint.position;
    }

    void Update()
    {
        if (raceEnded) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            raceStarted = true;
            Debug.Log("Race started!");
        }

        if (!raceStarted) return;

        foreach (Runner runner in racers) runner.ComputeDesiredOffset(racers);
        foreach (Runner runner in racers) runner.Tick(Time.deltaTime);

        ResolveOverlaps(racers); // keep this as a safety net underneath
        CheckRaceEnd();
    }

    void ResolveOverlaps(List<Runner> racers)
    {
        float minDistance = 0.8f;
        float maxLaneOffset = roadHalfWidth; // keep repeated pushes from drifting a runner off-track

        for (int i = 0; i < racers.Count; i++)
        {
            for (int j = i + 1; j < racers.Count; j++)
            {
                Runner a = racers[i];
                Runner b = racers[j];
                if (a.hasFinished || b.hasFinished) continue;

                Vector3 delta = a.transform.position - b.transform.position;
                float dist = delta.magnitude;

                if (dist < minDistance && dist > 0.001f)
                {
                    float overlap = minDistance - dist;

                    Vector3 tangentA = mainSpline.EvaluateTangent(a.t);
                    Vector3 perpA = new Vector3(-tangentA.y, tangentA.x, 0f).normalized;
                    float pushAmount = Vector3.Dot(delta.normalized, perpA) * overlap * 0.5f;

                    a.laneOffset = Mathf.Clamp(a.laneOffset + pushAmount, -maxLaneOffset, maxLaneOffset);
                    b.laneOffset = Mathf.Clamp(b.laneOffset - pushAmount, -maxLaneOffset, maxLaneOffset);
                }
            }
        }
    }

    void CheckRaceEnd()
    {
        foreach (Runner runner in racers)
        {
            if (!runner.hasFinished) return; // at least one runner still running
        }

        raceEnded = true;
        Debug.Log("Race finished! All runners reached the end.");
    }
}