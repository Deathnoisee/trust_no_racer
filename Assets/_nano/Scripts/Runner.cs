using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using SmallHedge.SoundManager;

public enum CheatType
{
    None,
    ShortcutCut,
    SpeedBoost,
    DisappearBoost,
    Injure,
    InfoMismatch
}
[System.Serializable]
public class KmSplit
{
    public int kmIndex;       // which km this is (0 = start to knot 1, etc.)
    public float timeSeconds; // how long this km took
    public float paceKmh;     // convenience: km/h for this split
}
[System.Serializable]
public struct TrajectoryPoint
{
    public float time;
    public Vector3 position;
}



[System.Serializable]
public class CheatConfig
{
    public CheatType type = CheatType.ShortcutCut;
    [Range(0f, 1f)] public float chance = 1f; // chance the assigned runner actually cheats this race

    [Tooltip("Which lap this cheat triggers on (1 = first lap). Only matters when the level's " +
             "lap count is greater than 1 — clamped to the runner's actual total laps, so a " +
             "'lap 3' cheat on a 2-lap level just fires on the last lap instead.")]
    [Min(1)] public int cheatOnLap = 1;

    [Header("Shortcut Cut settings")]
    public int shortcutFromKnot = 3;
    public int shortcutSkipCount = 2;

    [Header("Disappear Boost settings")]
    [Range(0f, 1f)] public float disappearTriggerProgress = 0.4f;
    public float disappearDuration = 2f;
    public float disappearBoostMultiplier = 2f;

    [Header("Injure settings")]
    [Range(0f, 1f)] public float injureTriggerProgress = 0.3f;
    public float injureRange = 0.03f; // how close in t counts as "close enough to injure"

    [Header("Speed Boost settings")]
    [Tooltip("Progress along the track (0-1) at which the boost kicks in.")]
    [Range(0f, 1f)] public float triggerProgress = 0.3f;
    public float boostMultiplier = 2.5f;
    public float boostDuration = 1.5f;
}

// Runtime state for the single cheat a Runner has been assigned. Kept separate from
// CheatConfig so the same designer-authored config asset can be reused across runners/levels.
public class ActiveCheat
{
    public CheatConfig config;
    public bool hasTriggered;

    // which lap (0-indexed) this cheat is allowed to fire on, resolved once at spawn
    // time from config.cheatOnLap and clamped to the runner's actual total laps
    public int resolvedCheatLap;

    // shortcut cut runtime
    public int resolvedFromKnot;
    public int resolvedToKnot;
    public float shortcutStartT;
    public float shortcutEndT;
    public bool isShortcutActive;
    public Vector3 shortcutStartPos;
    public Vector3 shortcutEndPos;
    public float shortcutProgress;

    // speed boost runtime
    public float boostTimeRemaining;

    // disappear boost runtime
    public float disappearTimeRemaining;
    public bool isDisappeared;

    // injure runtime
    public bool hasInjured;
}

public class Runner : MonoBehaviour
{

    public RunnerData runnerData;

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

    [Header("Pace Tracking")]
    public List<KmSplit> kmSplits = new List<KmSplit>();

    private float[] kmThresholdsT; // t value of each knot, precomputed once
    private int nextKmIndex = 0;
    private float lastKmCrossTime = 0f; // race time when the last km boundary was crossed

    [Header("Trajectory Recording")]
    public float trajectorySampleInterval = 1f; // seconds between recorded points
    public List<TrajectoryPoint> trajectory = new List<TrajectoryPoint>();

    private float nextTrajectorySampleTime = 0f;

    [Header("Pace Variation")]
    public float minPaceMultiplier = 0.7f;
    public float maxPaceMultiplier = 1.3f;
    public float paceChangeIntervalMin = 3f;
    public float paceChangeIntervalMax = 7f;
    private float nextPaceChangeTime;

    [Header("Lane")]
    public float laneOffset = 0f;
    public float laneWobbleAmount = 0.1f;
    public float laneWobbleSpeed = 1f;
    private float laneWobblePhase;

    [Header("Racing Line")]
    public float preferredOffset = 0f;
    public float desiredLaneOffset = 0f;
    public float laneEaseSpeed = 2f;
    public float neighborTDistance = 0.02f;
    public float neighborLateralThreshold = 0.5f;
    public float steerAwayAmount = 0.4f;

    [Header("Racing Line - Corner Cutting")]
    [Tooltip("How far ahead in t to look when judging whether a corner is coming up.")]
    public float cornerLookahead = 0.03f;
    [Tooltip("How strongly runners lean toward the inside of a corner, as a fraction of road half-width. " +
             "NOTE: which side is 'inside' depends on your spline's winding direction — if runners hug " +
             "the outside instead, flip the sign in ComputeCornerHugOffset().")]
    [Range(0f, 1f)] public float cornerHugStrength = 0.6f;

    [Header("Racing Line - Wander")]
    [Tooltip("Slow side-to-side roaming so runners don't all glue to one line down the straights.")]
    public float wanderAmplitude = 0.4f;
    public float wanderSpeed = 0.3f;
    private float wanderPhase;

    [Header("Road")]
    [Tooltip("How far from the spline centerline the road extends on either side. Every lane " +
             "target (preferred line, avoidance steering, overlap pushes) gets clamped to this, " +
             "so a runner spawned or pushed off-road always works its way back onto the road " +
             "instead of running a permanent off-road lane. Set by RaceManager per track.")]
    public float roadHalfWidth = 1.5f;

    [Header("Cheater Data (hidden from player during race)")]
    public bool isCheater = false;
    public bool isCheating = false; // true while a cheat is actively in effect (shortcut, speed boost, disappear, injure)
    [Tooltip("The single cheat this runner performs, assigned by RaceManager from the current " +
             "LevelConfig. A runner can only ever have one — never a list.")]
    public CheatConfig assignedCheat;

    private ActiveCheat activeCheat; // null if not a cheater, no cheat assigned, or the chance roll failed

    [Header("Laps")]
    [Tooltip("How many times this runner must loop mainSpline before the race counts them as " +
             "finished. Set by RaceManager from the current LevelConfig.")]
    public int totalLaps = 1;
    [HideInInspector] public int currentLap = 0;

    // Overall race completion, 0-1, across ALL laps (not just the current one around the
    // spline). Handy for progress bars / minimaps — t alone only tells you where you are
    // on the current lap.
    public float RaceProgress => Mathf.Clamp01((currentLap + Mathf.Clamp01(t)) / totalLaps);

    [HideInInspector] public float t = 0f;
    [HideInInspector] public bool hasFinished = false;
    [HideInInspector] public bool hasStarted = false;
    public RunnerVisuals visuals;

    [HideInInspector] public bool isInjured = false;


    [Header("Rotation")]
    public float rotationSmoothing = 8f;


    public SpriteRenderer tshirtRenderer;
    // Call this right after Instantiate (before Start runs, since Start is deferred to
    // just before the next Update) to place the runner at a designated spawn location
    // and have it continue running from there with no snap/teleport.

    public void SetSpawn(float startT, float startLaneOffset, Vector3 worldPos)
    {
        t = startT;
        // clamp to the road: a spawn point placed off-road still starts the runner there
        // visually, but its target lane is pulled onto the road, so it eases back on over
        // the first stretch instead of permanently running parallel to the actual track.
        laneOffset = Mathf.Clamp(startLaneOffset, -roadHalfWidth, roadHalfWidth);
        preferredOffset = laneOffset;
        transform.position = worldPos;
    }


    public void Injure()
    {
        if (isInjured) return; // don't double-trigger
        isInjured = true;

        // stop moving immediately — the Tick() guard below prevents further progress
        currentSpeedMultiplier = 0f;
        targetSpeedMultiplier = 0f;

        if (visuals != null) visuals.PlayInjuredDisappearAnimation();
    }
    void Start()
    {
        //gameObject.GetComponentInChildren<SpriteRenderer>().color = runnerColor;
        tshirtRenderer.color = runnerColor;
        visuals = GetComponentInChildren<RunnerVisuals>();
        nextPaceChangeTime = Time.time + UnityEngine.Random.Range(paceChangeIntervalMin, paceChangeIntervalMax);
        laneWobblePhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        wanderPhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

        // reaffirm desired/actual lane offset from preferredOffset (SetSpawn already set
        // preferredOffset for spawn-placed runners, so this is a no-op harmless overwrite)
        desiredLaneOffset = preferredOffset;
        laneOffset = preferredOffset;

        BuildActiveCheat();

        int knotCount = mainSpline.Spline.Count;
        kmThresholdsT = new float[knotCount];
        for (int i = 0; i < knotCount; i++)
        {
            kmThresholdsT[i] = SplineKnotUtils.GetKnotT(mainSpline, i);
        }
        lastKmCrossTime = Time.time;
    }
    void RecordTrajectorySample(Vector3 position)
    {
        if (Time.time >= nextTrajectorySampleTime)
        {
            trajectory.Add(new TrajectoryPoint
            {
                time = Time.time,
                position = position
            });
            nextTrajectorySampleTime = Time.time + trajectorySampleInterval;
        }
    }
    void CheckKmCrossing()
    {
        if (nextKmIndex >= kmThresholdsT.Length) return; // already passed the last knot

        if (t >= kmThresholdsT[nextKmIndex])
        {
            float splitTime = Time.time - lastKmCrossTime;
            float paceKmh = splitTime > 0f ? (3600f / splitTime) : 0f; // 1 km per splitTime seconds -> km/h

            kmSplits.Add(new KmSplit
            {
                kmIndex = nextKmIndex,
                timeSeconds = splitTime,
                paceKmh = paceKmh
            });

            lastKmCrossTime = Time.time;
            nextKmIndex++;
        }
    }
    // call this right after a shortcut lands (t = activeShortcut.shortcutEndT)
    void HandleKmSkipAfterShortcut()
    {
        while (nextKmIndex < kmThresholdsT.Length && t >= kmThresholdsT[nextKmIndex])
        {
            kmSplits.Add(new KmSplit
            {
                kmIndex = nextKmIndex,
                timeSeconds = 0f,  // no real time recorded — this km was skipped
                paceKmh = -1f      // sentinel value flagging "no data / suspicious"
            });
            nextKmIndex++;
        }
    }
    void BuildActiveCheat()
    {
        activeCheat = null;
        if (!isCheater || assignedCheat == null) return;
        if (UnityEngine.Random.value > assignedCheat.chance) return; // rolled to run clean this race

        activeCheat = new ActiveCheat { config = assignedCheat };
        activeCheat.resolvedCheatLap = Mathf.Clamp(assignedCheat.cheatOnLap - 1, 0, Mathf.Max(totalLaps - 1, 0));

        if (assignedCheat.type == CheatType.ShortcutCut)
        {
            int knotCount = mainSpline.Spline.Count;
            // clamp so a badly configured knot range can't index out of bounds
            activeCheat.resolvedFromKnot = Mathf.Clamp(assignedCheat.shortcutFromKnot, 0, knotCount - 1);
            activeCheat.resolvedToKnot = Mathf.Clamp(assignedCheat.shortcutFromKnot + assignedCheat.shortcutSkipCount, 0, knotCount - 1);
            activeCheat.shortcutStartT = SplineKnotUtils.GetKnotT(mainSpline, activeCheat.resolvedFromKnot);
            activeCheat.shortcutEndT = SplineKnotUtils.GetKnotT(mainSpline, activeCheat.resolvedToKnot);
        }
    }

    // Call this on ALL runners BEFORE calling Tick() on any of them,
    // so decisions are based on last frame's positions consistently.
    public void ComputeDesiredOffset(List<Runner> allRunners)
    {
        UpdateInjureCheck(allRunners);
        float cornerBias = ComputeCornerHugOffset();
        float wander = Mathf.Sin(Time.time * wanderSpeed + wanderPhase) * wanderAmplitude;
        float baseTarget = preferredOffset + cornerBias + wander;

        // proportional avoidance instead of a hard on/off threshold: the closer another
        // runner is laterally, the harder we push away, so movement reads as "finding room"
        // rather than snapping between two fixed states.
        float avoidance = 0f;
        foreach (Runner other in allRunners)
        {
            if (other == this || other.hasFinished) continue;

            float tDiff = Mathf.Abs(other.t - t);
            if (tDiff > neighborTDistance) continue;

            float lateralDiff = baseTarget - other.laneOffset;
            float absDiff = Mathf.Abs(lateralDiff);
            if (absDiff < neighborLateralThreshold)
            {
                float closeness = 1f - (absDiff / neighborLateralThreshold); // 0 far -> 1 right on top
                float direction = lateralDiff >= 0f ? 1f : -1f;
                avoidance += direction * steerAwayAmount * closeness;
            }
        }

        desiredLaneOffset = Mathf.Clamp(baseTarget + avoidance, -roadHalfWidth, roadHalfWidth);
    }

    // Estimates whether a corner is coming up and how sharp it is, and returns a signed
    // lateral offset that leans toward the inside of it — the same trick real racers use
    // to shorten their line through a turn, which naturally reads as "cutting inside if
    // there's room" once combined with the neighbor avoidance above.
    float ComputeCornerHugOffset()
    {
        float aheadT = Mathf.Clamp01(t + cornerLookahead);
        Vector3 nowDir = ((Vector3)mainSpline.EvaluateTangent(t)).normalized;
        Vector3 aheadDir = ((Vector3)mainSpline.EvaluateTangent(aheadT)).normalized;

        // signed turn amount via 2D cross product: sign tells you which way the track is
        // curving, magnitude tells you how sharply.
        float turnAmount = nowDir.x * aheadDir.y - nowDir.y * aheadDir.x;
        float turnSharpness = Mathf.Clamp01(Mathf.Abs(turnAmount) * 10f); // tune the 10x to taste
        float sign = Mathf.Sign(turnAmount);

        return -sign * turnSharpness * cornerHugStrength * roadHalfWidth;
    }

    public void Tick(float deltaTime)
    {
        if (hasFinished || isInjured) return;

        hasStarted = true;

        if (Time.time >= nextPaceChangeTime)
        {
            targetSpeedMultiplier = UnityEngine.Random.Range(minPaceMultiplier, maxPaceMultiplier);
            nextPaceChangeTime = Time.time + UnityEngine.Random.Range(paceChangeIntervalMin, paceChangeIntervalMax);
        }
        currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetSpeedMultiplier, deltaTime * speedChangeSmoothing);

        ActiveCheat activeShortcut = GetOrTriggerShortcut();
        float speedBoostMultiplier = UpdateSpeedBoost(deltaTime);
        float disappearBoostMultiplier = UpdateDisappearBoost(deltaTime);
        float distance = baseSpeed * currentSpeedMultiplier * speedBoostMultiplier * disappearBoostMultiplier * deltaTime;

        Vector3 finalPos;

        if (activeShortcut != null)
        {
            float skipDistance = Vector3.Distance(activeShortcut.shortcutStartPos, activeShortcut.shortcutEndPos);
            activeShortcut.shortcutProgress += distance / Mathf.Max(skipDistance, 0.01f);

            if (activeShortcut.shortcutProgress >= 1f)
            {
                activeShortcut.shortcutProgress = 1f;
                activeShortcut.isShortcutActive = false;
                t = activeShortcut.shortcutEndT; // rejoin the normal spline at the landing knot
                if (t >= 1f) AdvanceLap(); // in case the cut lands past the finish line
            }

            finalPos = Vector3.Lerp(activeShortcut.shortcutStartPos, activeShortcut.shortcutEndPos, activeShortcut.shortcutProgress);

            Vector3 cutDir = (activeShortcut.shortcutEndPos - activeShortcut.shortcutStartPos).normalized;
            Vector3 perp = new Vector3(-cutDir.y, cutDir.x, 0f);
            if (cutDir.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(cutDir.y, cutDir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            laneOffset = Mathf.Lerp(laneOffset, desiredLaneOffset, deltaTime * laneEaseSpeed);
            float wobble = Mathf.Sin(Time.time * laneWobbleSpeed + laneWobblePhase) * laneWobbleAmount;
            finalPos += perp * (laneOffset + wobble);
        }
        else
        {
            float splineLength = mainSpline.CalculateLength();
            t += distance / splineLength;
            CheckKmCrossing();

            if (t >= 1f) AdvanceLap();

            Vector3 centerPos = mainSpline.EvaluatePosition(t);
            Vector3 tangent = mainSpline.EvaluateTangent(t);
            Vector3 perpendicular = new Vector3(-tangent.y, tangent.x, 0f).normalized;

            if (tangent.sqrMagnitude > 0.0001f)
            {
                float targetAngle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg - 90f;
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * rotationSmoothing);
            }

            laneOffset = Mathf.Lerp(laneOffset, desiredLaneOffset, deltaTime * laneEaseSpeed);
            float wobble = Mathf.Sin(Time.time * laneWobbleSpeed + laneWobblePhase) * laneWobbleAmount;
            finalPos = centerPos + perpendicular * (laneOffset + wobble);
        }

        transform.position = finalPos;
        RecordTrajectorySample(finalPos);
    }

    // Called whenever t crosses 1 (one full pass of mainSpline). On a looping track this
    // wraps back around for another lap instead of finishing immediately; only once
    // currentLap reaches totalLaps do we actually mark the runner finished. Requires
    // mainSpline to be a CLOSED spline, or the wrap will visibly jump.
    void AdvanceLap()
    {
        currentLap++;

        if (currentLap >= totalLaps)
        {
            t = 1f;
            hasFinished = true;
            CameraShake.instance.ShakeMedium();
            SoundManager.PlaySound(SoundType.Win);
            Debug.Log($"{runnerName} (bib {runnerBibNumber}) finished the race!");
            Debug.Log($"Km splits for {runnerName}:");
            foreach (KmSplit split in kmSplits)
            {
                Debug.Log($"Km {split.kmIndex + 1}: {split.timeSeconds:F2}s, pace {split.paceKmh:F2} km/h");
            }
            runnerData.trajectory = trajectory;
            runnerData.kmSplits = kmSplits;

        }
        else
        {
            t -= 1f; // keep the overshoot so speed reads smoothly across the lap seam
        }
    }

    // Returns the shortcut cheat while it's mid-cut, triggering it the moment we reach
    // its start knot. Null the rest of the time (including for non-shortcut cheaters).
    ActiveCheat GetOrTriggerShortcut()
    {
        if (activeCheat == null || activeCheat.config.type != CheatType.ShortcutCut) return null;

        if (activeCheat.isShortcutActive) return activeCheat;

        if (!activeCheat.hasTriggered && currentLap == activeCheat.resolvedCheatLap && t >= activeCheat.shortcutStartT)
        {
            activeCheat.hasTriggered = true;
            activeCheat.isShortcutActive = true;
            activeCheat.shortcutProgress = 0f;
            activeCheat.shortcutStartPos = SplineKnotUtils.GetKnotWorldPosition(mainSpline, activeCheat.resolvedFromKnot);
            activeCheat.shortcutEndPos = SplineKnotUtils.GetKnotWorldPosition(mainSpline, activeCheat.resolvedToKnot);
            isCheating = true;
            return activeCheat;
        }

        isCheating = false;
        return null;
    }

    // Triggers the speed boost once its progress threshold is reached, ticks it down,
    // and returns the multiplier to apply to movement this frame (1 = no boost active).
    float UpdateSpeedBoost(float deltaTime)
    {
        if (activeCheat == null || activeCheat.config.type != CheatType.SpeedBoost) return 1f;

        if (!activeCheat.hasTriggered && currentLap == activeCheat.resolvedCheatLap && t >= activeCheat.config.triggerProgress)
        {
            activeCheat.hasTriggered = true;
            activeCheat.boostTimeRemaining = activeCheat.config.boostDuration;

        }

        if (activeCheat.boostTimeRemaining > 0f)
        {
            activeCheat.boostTimeRemaining -= deltaTime;
            return activeCheat.config.boostMultiplier;
        }

        return 1f;
    }

    float UpdateDisappearBoost(float deltaTime)
    {
        if (activeCheat == null || activeCheat.config.type != CheatType.DisappearBoost) return 1f;

        if (!activeCheat.hasTriggered && currentLap == activeCheat.resolvedCheatLap && t >= activeCheat.config.disappearTriggerProgress)
        {
            activeCheat.hasTriggered = true;
            activeCheat.isDisappeared = true;
            activeCheat.disappearTimeRemaining = activeCheat.config.disappearDuration;

            SetVisible(false);
        }

        if (activeCheat.isDisappeared)
        {
            activeCheat.disappearTimeRemaining -= deltaTime;
            if (activeCheat.disappearTimeRemaining <= 0f)
            {
                activeCheat.isDisappeared = false;
                SetVisible(true);
            }
            return activeCheat.config.disappearBoostMultiplier;
        }

        return 1f;
    }

    void SetVisible(bool visible)
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.enabled = visible;
    }

    void UpdateInjureCheck(List<Runner> allRunners)
    {
        if (activeCheat == null || activeCheat.config.type != CheatType.Injure) return;
        if (activeCheat.hasInjured || currentLap != activeCheat.resolvedCheatLap || t < activeCheat.config.injureTriggerProgress) return;

        foreach (Runner other in allRunners)
        {
            if (other == this || other.isInjured || other.hasFinished) continue;

            if (Mathf.Abs(other.t - t) <= activeCheat.config.injureRange)
            {
                other.Injure();
                activeCheat.hasInjured = true; // this cheater can only injure once, ever

                break;
            }
        }
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

    // Finds the closest point on the spline to a world-space position, and how far
    // sideways (perpendicular to the track) that position sits from the spline.
    // Used to turn an arbitrary designer-placed spawn point into a (t, laneOffset) pair.
    public static void GetNearestTAndLateralOffset(SplineContainer container, Vector3 worldPos, out float t, out float lateralOffset)
    {
        float3 localPos = container.transform.InverseTransformPoint(worldPos);
        SplineUtility.GetNearestPoint(container.Spline, localPos, out float3 nearest, out t);

        Vector3 nearestWorld = container.transform.TransformPoint((Vector3)nearest);
        Vector3 tangent = container.EvaluateTangent(t);
        Vector3 perpendicular = new Vector3(-tangent.y, tangent.x, 0f).normalized;

        lateralOffset = Vector3.Dot(worldPos - nearestWorld, perpendicular);
    }
}