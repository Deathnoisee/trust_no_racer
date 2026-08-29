using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class RaceDemoManager : MonoBehaviour
{
     public GameObject runnerPrefab;
    public SplineContainer spline;
    public Transform[] spawnPoints;
    public Color[] runnerColors;
    public int runnerCount = 6;

    private List<Runner> demoRunners = new List<Runner>();

    void Awake()
    { for (int i = 0; i < runnerCount; i++)
        {
            Vector3 spawnPos = spawnPoints[i % spawnPoints.Length].position;
            GameObject obj = Instantiate(runnerPrefab, spawnPos, Quaternion.identity);
            Runner runner = obj.GetComponent<Runner>();
            runner.runnerColor = runnerColors[i];

            runner.runnerBibNumber = i;
            runner.runnerName = "Demo " + i;
            
            runner.mainSpline = spline;
            runner.baseSpeed = Random.Range(3.5f, 4.5f);
            runner.totalLaps = int.MaxValue; // effectively infinite — never "finishes"
            runner.isCheater = false; // keep it clean, no cheat visuals on a menu screen

            SplineKnotUtils.GetNearestTAndLateralOffset(spline, spawnPos, out float _, out float spawnLateral);
            runner.SetSpawn(0f, spawnLateral, spawnPos);

            demoRunners.Add(runner);
        }
        
    }
    void Start()
    {
       
    }

    void Update()
    {
        foreach (Runner runner in demoRunners) runner.ComputeDesiredOffset(demoRunners);
        foreach (Runner runner in demoRunners) runner.Tick(Time.deltaTime);
    }
}
