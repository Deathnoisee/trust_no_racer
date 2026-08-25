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

    [Header("Start Formation")]
    public int runnersPerRow = 4;
    public float rowSpacing = 0.6f;    // distance between rows (front-to-back, along track direction)
    public float columnSpacing = 0.5f;



    private bool raceStarted = false;



    [Header("Race Config")]
    public int totalRacers = 8;
    public int cheaterCount = 2;
    public float laneSpread = 1.2f;

    private List<Runner> racers = new List<Runner>();
    private bool raceEnded = false;

    void Start()
    {
        SpawnRacers();
    }

    void SpawnRacers()
    {
        HashSet<int> cheaterIndices = new HashSet<int>();
        while (cheaterIndices.Count < cheaterCount)
        {
            cheaterIndices.Add(Random.Range(0, totalRacers));
        }

        // get the track's forward direction at the start, so the formation aligns with the road
        Vector3 startTangent = mainSpline.EvaluateTangent(0f);
        Vector3 forward = ((Vector3)startTangent).normalized;
        Vector3 sideways = new Vector3(-forward.y, forward.x, 0f); // perpendicular to forward

        for (int i = 0; i < totalRacers; i++)
        {
            int row = i / runnersPerRow;
            int col = i % runnersPerRow;

            // center the columns around the start point instead of offsetting only to one side
            float colOffset = (col - (runnersPerRow - 1) / 2f) * columnSpacing;
            float rowOffset = -row * rowSpacing; // rows behind the start line, not ahead of it

            Vector3 spawnPos = startPoint.position + sideways * colOffset + forward * rowOffset;

            GameObject obj = Instantiate(runnerPrefab, spawnPos, Quaternion.identity);
            Runner runner = obj.GetComponent<Runner>();

            runner.runnerBibNumber = i;
            runner.runnerName = "Runner " + i;
            runner.runnerColor = runnerColors[i % runnerColors.Length];
            runner.mainSpline = mainSpline;
            runner.baseSpeed = Random.Range(3.5f, 4.5f);
            runner.isCheater = cheaterIndices.Contains(i);
            runner.laneOffset = Random.Range(-laneSpread / 2f, laneSpread / 2f);

            racers.Add(runner);
        }
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
        foreach (Runner runner in racers)
        {
            runner.ComputeDesiredOffset(racers);
        }
        foreach (Runner runner in racers)
        {
            runner.Tick(Time.deltaTime);
        }
        ResolveOverlaps(racers); // keep this as a safety net underneath


        CheckRaceEnd();
    }

    // call this after normal Tick() movement, in RaceController's Update loop
    void ResolveOverlaps(List<Runner> racers)
    {
        float minDistance = 1f;

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

                    // get perpendicular direction at each racer's own position on the spline
                    Vector3 tangentA = mainSpline.EvaluateTangent(a.t);
                    Vector3 perpA = new Vector3(-tangentA.y, tangentA.x, 0f).normalized;

                    // project world-space delta onto the perpendicular to get a signed sideways push
                    float pushAmount = Vector3.Dot(delta.normalized, perpA) * overlap * 0.5f;

                    a.laneOffset += pushAmount;
                    b.laneOffset -= pushAmount;
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
