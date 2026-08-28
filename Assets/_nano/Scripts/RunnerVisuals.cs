using UnityEngine;
using DG.Tweening;
using SmallHedge.SoundManager;

public class RunnerVisuals : MonoBehaviour
{
    public Runner runner;

    [Header("Stride")]
    public float strideFrequencyPerSpeed = 1.2f;
    public float strideSquash = 0.08f;

    [Header("Bob")]
    public float bobAmount = 0.03f;

    [Header("Lean")]
    public float maxLeanAngle = 12f;
    public float leanSmoothing = 6f;
    public float leanDeadZone = 0.02f;

    [Header("Acceleration Burst Punch")]
    public float burstThreshold = 0.25f;
    public float punchScaleAmount = 0.35f;
    public float punchDuration = 0.25f;

    [Header("Cheat Activation Burst")]
    public float cheatPunchScaleAmount = 0.7f; // bigger than your normal accel punch
    public float cheatPunchDuration = 0.35f;
    public Color cheatPunchFlashColor = Color.red; // optional visual tell
    public float cheatFlashDuration = 0.15f;

    private float strideTimer = 0f;
    private float currentLean = 0f;
    private Vector3 baseScale;
    private float lastTargetSpeed;


    // DOTween drives this value only — stride math never touches it directly
    private Vector3 punchScaleOffset = Vector3.zero;
    private Tween punchTween;

    void Awake()
    {
        baseScale = transform.localScale;
        lastTargetSpeed = runner.targetSpeedMultiplier;
    }

    void Update()
    {
        if (runner.hasFinished)
            return;
        float targetDelta = runner.targetSpeedMultiplier - lastTargetSpeed;
        if (targetDelta > burstThreshold)
        {
            CameraShake.instance.ShakeSmall();
            TriggerBurstPunch();
        }
        lastTargetSpeed = runner.targetSpeedMultiplier;

        float speed = runner.baseSpeed * runner.currentSpeedMultiplier;
        strideTimer += Time.deltaTime * speed * strideFrequencyPerSpeed;
        float stridePulse = Mathf.Sin(strideTimer * Mathf.PI * 2f);

        float scaleY = 1f + stridePulse * strideSquash;
        float scaleX = 1f - stridePulse * strideSquash * 0.5f;

        // combine stride scale with punch offset ADDITIVELY, so DOTween's animated
        // punchScaleOffset and the per-frame stride math never overwrite each other
        Vector3 strideScale = new Vector3(baseScale.x * scaleX, baseScale.y * scaleY, baseScale.z);
        transform.localScale = strideScale + punchScaleOffset;

        float bob = Mathf.Abs(stridePulse) * bobAmount;
        transform.localPosition = new Vector3(0f, bob, 0f);

        float steerDelta = runner.desiredLaneOffset - runner.laneOffset;
        if (Mathf.Abs(steerDelta) < leanDeadZone) steerDelta = 0f;
        float targetLean = Mathf.Clamp(-steerDelta * maxLeanAngle, -maxLeanAngle, maxLeanAngle);
        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSmoothing);
        transform.localRotation = Quaternion.Euler(0f, 0f, currentLean);

    }


    void TriggerBurstPunch()
    {
        punchTween?.Kill();
        punchScaleOffset = Vector3.zero;
        SoundManager.PlaySound(SoundType.PowerUp);
        runner.TriggerTrail(0.3f); // optional trail effect for extra visual feedback

        // DOTween animates punchScaleOffset itself (a plain Vector3 field),
        // NOT transform.localScale directly — that's what lets stride math
        // keep writing to localScale every frame without the two fighting.
        punchTween = DOTween.To(
            () => punchScaleOffset,
            x => punchScaleOffset = x,
            Vector3.one * punchScaleAmount,
            punchDuration
        ).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
    }

    public void TriggerCheatBurst()
    {
        punchTween?.Kill();
        punchScaleOffset = Vector3.zero;
        SoundManager.PlaySound(SoundType.PowerUp);

        punchTween = DOTween.To(
            () => punchScaleOffset,
            x => punchScaleOffset = x,
            Vector3.one * cheatPunchScaleAmount,
            cheatPunchDuration
        ).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutBack); // OutBack gives a snappier "pop" than OutQuad

        // optional: quick color flash for extra readability, since this is a bigger/rarer moment
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.DOColor(cheatPunchFlashColor, cheatFlashDuration)
              .SetLoops(2, LoopType.Yoyo)
              .OnComplete(() => sr.color = original);
        }
    }



    // in RunnerVisuals.cs
    public void PlayInjuredDisappearAnimation(float duration = 0.6f)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack));
        seq.Join(sr.DOFade(0f, duration));
    }
}