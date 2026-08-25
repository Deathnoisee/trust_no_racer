using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
public class Runner : MonoBehaviour
{
    [Header("Identity")]
    public int runnerBibNumber;
    public string runnerName;
    public Color runnerColor;

    [Header("Path")]
    public SplineContainer mainSpline;

    [Header("Pace")]
    public float baseSpeed = 2f;
    public float currentSpeedMultiplier = 1f;
    public float targetSpeedMultiplier = 1f;
    public float speedChangeSmoothing = 1.5f;

    [Header("Pace Variation")]
    public float minPaceMultiplier = 0.7f;
    public float maxPaceMultiplier = 1.3f;
    public float paceChangeIntervalMin = 3f;
    public float paceChangeIntervalMax = 7f;
    private float nextPaceChangeTime;


    [Header("Shortcut / Cheating Behavior")]
    public int shortcutFromKnot = 3;   // knot index where the skip starts
    public int shortcutSkipCount = 2;  // how many knots to skip ahead (2 = skip one knot)
    public float shortcutChance = 1f; // chance a cheater will take the shortcut
    private bool hasTakenShortcut = false;
    private bool willTakeShortcut = false;
    private bool isShortcutting = false;
    private float shortcutStartT;
    private float shortcutEndT;
    private Vector3 shortcutStartPos;
    private Vector3 shortcutEndPos;
    private float shortcutProgress = 0f; // 0-1 through the skip itself


    [Header("Lane")]
    public float laneOffset = 0f;
    public float laneWobbleAmount = 0.1f;
    public float laneWobbleSpeed = 1f;
    private float laneWobblePhase;

    [Header("Racing Line")]
    public float preferredOffset = 0f;     // this runner's ideal line on the track
    public float desiredLaneOffset = 0f;   // computed each frame from preference + nearby runners
    public float laneEaseSpeed = 2f;       // how quickly laneOffset chases desiredLaneOffset
    public float neighborTDistance = 0.02f; // how close in progress counts as "nearby"
    public float neighborLateralThreshold = 0.5f; // how close laterally before yielding
    public float steerAwayAmount = 0.4f;

    [Header("Cheater Data (hidden from player during race)")]
    public bool isCheater = false;

    [HideInInspector] public float t = 0f;
    [HideInInspector] public bool hasFinished = false;

    void Start()
    {
        willTakeShortcut = isCheater && UnityEngine.Random.value < shortcutChance;
        if (willTakeShortcut)
        {
            shortcutStartT = SplineKnotUtils.GetKnotT(mainSpline, shortcutFromKnot);
            shortcutEndT = SplineKnotUtils.GetKnotT(mainSpline, shortcutFromKnot + shortcutSkipCount);
        }
        gameObject.GetComponent<SpriteRenderer>().color = runnerColor;
        nextPaceChangeTime = Time.time + UnityEngine.Random.Range(paceChangeIntervalMin, paceChangeIntervalMax);
        laneWobblePhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

        // start the desired offset at the preferred line so runners don't lerp from 0 at spawn
        desiredLaneOffset = preferredOffset;
        laneOffset = preferredOffset;
    }

    // Call this on ALL runners BEFORE calling Tick() on any of them,
    // so decisions are based on last frame's positions consistently.
    public void ComputeDesiredOffset(List<Runner> allRunners)
    {
        desiredLaneOffset = preferredOffset;

        foreach (Runner other in allRunners)
        {
            if (other == this || other.hasFinished) continue;

            float tDiff = Mathf.Abs(other.t - t);
            if (tDiff > neighborTDistance) continue; // only care about runners close in progress

            float lateralDiff = Mathf.Abs(other.laneOffset - preferredOffset);
            if (lateralDiff < neighborLateralThreshold)
            {
                // someone's occupying my preferred line — steer to whichever side has more room
                float pushAway = (preferredOffset - other.laneOffset >= 0) ? steerAwayAmount : -steerAwayAmount;
                desiredLaneOffset = preferredOffset + pushAway;
            }
        }
    }
    public void Tick(float deltaTime)
    {
        if (hasFinished) return;

        if (Time.time >= nextPaceChangeTime)
        {
            targetSpeedMultiplier = UnityEngine.Random.Range(minPaceMultiplier, maxPaceMultiplier);
            nextPaceChangeTime = Time.time + UnityEngine.Random.Range(paceChangeIntervalMin, paceChangeIntervalMax);
        }

        currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetSpeedMultiplier, deltaTime * speedChangeSmoothing);
        float distance = baseSpeed * currentSpeedMultiplier * deltaTime;

        // trigger the shortcut once we reach the start knot's t
        if (willTakeShortcut && !hasTakenShortcut && !isShortcutting && t >= shortcutStartT)
        {
            isShortcutting = true;
            hasTakenShortcut = true;
            shortcutStartPos = SplineKnotUtils.GetKnotWorldPosition(mainSpline, shortcutFromKnot);
            shortcutEndPos = SplineKnotUtils.GetKnotWorldPosition(mainSpline, shortcutFromKnot + shortcutSkipCount);
            shortcutProgress = 0f;
        }

        Vector3 finalPos;

        if (isShortcutting)
        {
            float skipDistance = Vector3.Distance(shortcutStartPos, shortcutEndPos);
            shortcutProgress += distance / Mathf.Max(skipDistance, 0.01f);

            if (shortcutProgress >= 1f)
            {
                shortcutProgress = 1f;
                isShortcutting = false;
                t = shortcutEndT; // rejoin the normal spline at the landing knot
            }

            finalPos = Vector3.Lerp(shortcutStartPos, shortcutEndPos, shortcutProgress);

            // lane offset still applies, but skip the tangent-based perpendicular calc during the cut
            // (straight-line lerp direction acts as our "tangent" here)
            Vector3 cutDir = (shortcutEndPos - shortcutStartPos).normalized;
            Vector3 perp = new Vector3(-cutDir.y, cutDir.x, 0f);
            laneOffset = Mathf.Lerp(laneOffset, desiredLaneOffset, deltaTime * laneEaseSpeed);
            float wobble = Mathf.Sin(Time.time * laneWobbleSpeed + laneWobblePhase) * laneWobbleAmount;
            finalPos += perp * (laneOffset + wobble);
        }
        else
        {
            float splineLength = mainSpline.CalculateLength();
            t += distance / splineLength;

            if (t >= 1f) { t = 1f; hasFinished = true; }

            Vector3 centerPos = mainSpline.EvaluatePosition(t);
            Vector3 tangent = mainSpline.EvaluateTangent(t);
            Vector3 perpendicular = new Vector3(-tangent.y, tangent.x, 0f).normalized;

            laneOffset = Mathf.Lerp(laneOffset, desiredLaneOffset, deltaTime * laneEaseSpeed);
            float wobble = Mathf.Sin(Time.time * laneWobbleSpeed + laneWobblePhase) * laneWobbleAmount;
            finalPos = centerPos + perpendicular * (laneOffset + wobble);
        }

        transform.position = finalPos;
    }
}

public static class SplineKnotUtils
{
    public static float GetKnotT(SplineContainer container, int knotIndex)
    {
        float3 knotLocalPos = container.Spline[knotIndex].Position;
        SplineUtility.GetNearestPoint(container.Spline, knotLocalPos, out float3 nearest, out float t);
        return t;
    }

    public static Vector3 GetKnotWorldPosition(SplineContainer container, int knotIndex)
    {
        float3 localPos = container.Spline[knotIndex].Position;
        return container.transform.TransformPoint((Vector3)localPos);
    }
}
