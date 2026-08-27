using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    [Header("Default Shake Settings")]
    public float defaultDuration = 0.3f;
    public float defaultStrength = 0.5f;
    public int defaultVibrato = 10;
    public float defaultRandomness = 90f;

    private Vector3 originalPosition;
    private Tween shakeTween;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        originalPosition = transform.localPosition;
    }

    
    public void Shake()
    {
        Shake(defaultDuration, defaultStrength, defaultVibrato, defaultRandomness);
    }

    
    public void Shake(float duration, float strength, int vibrato = 10, float randomness = 90f)
    {
        // Kill any shake currently in progress so shakes don't stack/compound oddly
        if (shakeTween != null && shakeTween.IsActive())
        {
            shakeTween.Kill();
            transform.localPosition = originalPosition;
        }

        shakeTween = transform.DOShakePosition(duration, strength, vibrato, randomness)
            .OnComplete(() => transform.localPosition = originalPosition);
    }


    public void ShakeSmall() => Shake(0.15f, 0.15f, 8, 90f);
    public void ShakeMedium() => Shake(0.3f, 0.4f, 10, 90f);
    public void ShakeBig() => Shake(0.5f, 0.8f, 14, 90f);
}