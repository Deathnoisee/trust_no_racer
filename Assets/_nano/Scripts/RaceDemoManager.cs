using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class RaceDemoManager : MonoBehaviour
{
    public GameObject runnerPrefab;
    public SplineContainer spline;
    public Transform[] spawnPoints;
    public Color[] runnerColors;
    public int runnerCount = 6;

    private List<Runner> demoRunners = new List<Runner>();

    void Awake()
    {
        if (runnerPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("RaceDemoManager: Missing runnerPrefab or spawnPoints!");
            return;
        }

        bool hasValidSpline = spline != null && spline.Spline != null && spline.Spline.Count > 0;

        for (int i = 0; i < runnerCount; i++)
        {
            Vector3 spawnPos = spawnPoints[i % spawnPoints.Length].position;
            GameObject obj = Instantiate(runnerPrefab, spawnPos, Quaternion.identity);
            Runner runner = obj.GetComponent<Runner>();

            if (runnerColors != null && runnerColors.Length > 0)
            {
                runner.runnerColor = runnerColors[i % runnerColors.Length];
            }

            runner.runnerBibNumber = i;
            runner.runnerName = "Demo " + i;
            runner.mainSpline = spline;
            runner.baseSpeed = UnityEngine.Random.Range(3.5f, 4.5f);
            runner.totalLaps = int.MaxValue;
            runner.isCheater = false;

            float spawnLateral = 0f;

            if (hasValidSpline)
            {
                // WebGL-safe custom nearest point & lateral offset calculation
                spawnLateral = GetLateralOffsetSafe(spline, spawnPos);
            }

            runner.SetSpawn(0f, spawnLateral, spawnPos);
            demoRunners.Add(runner);
        }
    }

    void Update()
    {
        foreach (Runner runner in demoRunners) runner.ComputeDesiredOffset(demoRunners);
        foreach (Runner runner in demoRunners) runner.Tick(Time.deltaTime);
    }

    /// <summary>
    /// Bypasses SplineUtility.GetNearestPoint to avoid Unity 6 WebGL IL2CPP generic sharing crashes.
    /// </summary>
    private float GetLateralOffsetSafe(SplineContainer container, Vector3 worldPosition, int resolution = 100)
    {
        try
        {
            Matrix4x4 localToWorld = container.transform.localToWorldMatrix;
            Vector3 localPos = container.transform.InverseTransformPoint(worldPosition);

            float bestT = 0f;
            float minDistanceSq = float.MaxValue;
            Vector3 nearestLocalPoint = Vector3.zero;

            // Step 1: Sample the spline linearly to find the closest t value
            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                float3 evalPos = container.EvaluatePosition(t);
                float distSq = Vector3.SqrMagnitude((Vector3)evalPos - localPos);

                if (distSq < minDistanceSq)
                {
                    minDistanceSq = distSq;
                    bestT = t;
                    nearestLocalPoint = (Vector3)evalPos;
                }
            }

            // Step 2: Evaluate forward/tangent vectors at bestT using non-generic methods
            float3 tangent = container.EvaluateTangent(bestT);
            Vector3 worldForward = localToWorld.MultiplyVector((Vector3)tangent).normalized;

            if (worldForward == Vector3.zero)
                worldForward = container.transform.forward;

            // Step 3: Compute lateral offset vector
            Vector3 nearestWorldPoint = localToWorld.MultiplyPoint3x4(nearestLocalPoint);
            Vector3 offsetVector = worldPosition - nearestWorldPoint;
            Vector3 worldRight = Vector3.Cross(Vector3.up, worldForward).normalized;

            return Vector3.Dot(offsetVector, worldRight);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Spline evaluation skipped on WebGL: {ex.Message}");
            return 0f;
        }
    }
}