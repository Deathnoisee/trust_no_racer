using System.Collections.Generic;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.Splines;

public class RaceManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject runnerPrefab;


    [Header("Tracks (all tracks in the scene, active or not)")]
    public TrackSetup[] allTracks;

    [Header("Colors")]
    public Color[] runnerColors;

    [Header("Road")]

    public float roadHalfWidth = 1.5f;

    [Header("Levels")]

    public List<LevelConfig> levels = new List<LevelConfig>();


    public int currentLevelIndex = 0;

    public GameObject startButton;

    [Header("Sound")]
    public float bumpSoundCooldown = 0.6f; // min time between bump sounds for the same pair
    private Dictionary<(int, int), float> lastBumpTime = new Dictionary<(int, int), float>();


    public int cheaterCount = 0;

    private bool raceStarted = false;
    private bool raceEnded = false;
    private List<Runner> racers = new List<Runner>();
    private TrackSetup activeTrack;


    public event System.Action OnRaceEnded;

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
        startButton.SetActive(true);
        currentLevelIndex = levelIndex;
        raceStarted = false;
        raceEnded = false;

        foreach (Runner runner in racers)
        {
            if (runner != null) Destroy(runner.gameObject);
        }
        racers.Clear();

        ActivateTrackForCurrentLevel();
        SpawnRacers();
    }

    void ActivateTrackForCurrentLevel()
    {
        LevelConfig level = CurrentLevel;
        string wantedTrackName = level != null ? level.trackName : null;

        activeTrack = null;
        foreach (TrackSetup track in allTracks)
        {
            bool shouldBeActive = (track.trackName == wantedTrackName);
            track.SetActive(shouldBeActive);
            if (shouldBeActive) activeTrack = track;
        }

        if (activeTrack == null && allTracks.Length > 0)
        {
            // fallback: no match found (e.g. empty trackName) — just use the first track
            activeTrack = allTracks[0];
            activeTrack.SetActive(true);
        }
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
        if (activeTrack == null)
        {
            Debug.LogError("No active track set — can't spawn racers.");
            return;
        }

        LevelConfig level = CurrentLevel;
        int racerCount = level != null ? level.totalRacers : 8;
        List<CheatConfig> cheatsToAssign = level != null ? level.cheaterCheats : new List<CheatConfig>();

        Dictionary<int, CheatConfig> cheatByRacerIndex = AssignCheatsToRacers(cheatsToAssign, racerCount);

        RunnersGenerator.instance.EmptyList();

        for (int i = 0; i < racerCount; i++)
        {
            Vector3 spawnPos = GetSpawnPosition(i);

            GameObject obj = Instantiate(runnerPrefab, spawnPos, Quaternion.identity);
            Runner runner = obj.GetComponent<Runner>();

            runner.runnerBibNumber = i;
            runner.runnerName = "Runner " + i;


            runner.mainSpline = activeTrack.spline;
            runner.baseSpeed = Random.Range(3.5f, 4.5f);
            runner.roadHalfWidth = roadHalfWidth;
            runner.totalLaps = level != null ? level.lapCount : 1;

            if (cheatByRacerIndex.TryGetValue(i, out CheatConfig cheat))
            {
                runner.isCheater = true;
                runner.assignedCheat = cheat;
                runner.runnerData = RunnersGenerator.instance.GeneratePerson(cheat.type);
            }
            else
            {
                runner.runnerData = RunnersGenerator.instance.GeneratePerson(CheatType.None);
            }
            runner.runnerColor = runner.runnerData.shirtColor;
            SplineKnotUtils.GetNearestTAndLateralOffset(activeTrack.spline, spawnPos, out float spawnT, out float spawnLateral);
            runner.SetSpawn(0f, spawnLateral, spawnPos);

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
        if (activeTrack.spawnPoints != null && index < activeTrack.spawnPoints.Length && activeTrack.spawnPoints[index] != null)
        {
            return activeTrack.spawnPoints[index].position;
        }
        return activeTrack.startPoint.position;
    }


    public void StartRace()
    {

        Camera.main.GetComponent<EdgeScrollCamera>().enabled = false; // Disable edge scrolling when the race starts
        Camera.main.transform.position = Vector3.zero; // Center camera on the start point
        SoundManager.StopMusic();
        SoundManager.StopAmbiance();
        SoundManager.PlaySound(SoundType.Music, null, 1f);
        raceStarted = true;
        startButton.SetActive(false);

    }


    void Update()
    {
        if (raceEnded) return;
        /*
            if (Input.GetKeyDown(KeyCode.Space))
            {
                raceStarted = true;
                Debug.Log("Race started!");
            }*/

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

                    Vector3 tangentA = activeTrack.spline.EvaluateTangent(a.t);
                    Vector3 perpA = new Vector3(-tangentA.y, tangentA.x, 0f).normalized;
                    float pushAmount = Vector3.Dot(delta.normalized, perpA) * overlap * 0.5f;

                    a.laneOffset = Mathf.Clamp(a.laneOffset + pushAmount, -maxLaneOffset, maxLaneOffset);
                    b.laneOffset = Mathf.Clamp(b.laneOffset - pushAmount, -maxLaneOffset, maxLaneOffset);
                    TryPlayBumpSound(a.runnerBibNumber, b.runnerBibNumber);
                }
            }
        }
    }
    void TryPlayBumpSound(int idA, int idB)
    {
        var key = idA < idB ? (idA, idB) : (idB, idA); // order-independent key

        if (lastBumpTime.TryGetValue(key, out float lastTime) && Time.time - lastTime < bumpSoundCooldown)
            return; // still on cooldown for this specific pair

        lastBumpTime[key] = Time.time;
        SoundManager.PlaySound(SoundType.Bump);
    }

    void CheckRaceEnd()
    {
        foreach (Runner runner in racers)
        {
            if (!runner.hasFinished) return; // at least one runner still running
        }

        raceEnded = true;
        // clear the list to avoid further updates
        foreach (Runner runner in racers)
        {
            if (runner != null)
            {
                Destroy(runner.gameObject);
            }
        }
        racers.Clear();
        Camera.main.GetComponent<EdgeScrollCamera>().enabled = true; // Re-enable edge scrolling after the race ends
        SoundManager.StopMusic();
        SoundManager.PlayMusic(SoundType.Jazz, 1f);
        SoundManager.StartAmbiance(SoundType.Ambient, 1f);


        Debug.Log("Race finished! All runners reached the end.");
        OnRaceEnded?.Invoke();


    }
}