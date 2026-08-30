using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarsGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float animationDuration = 0.6f;
    [SerializeField] private float delayBetweenStars = 0.25f;
    [SerializeField] private Ease moveEase = Ease.OutBack;

    [Header("Spawn Offset")]
    [Tooltip("Local position offset relative to each empty star where the filled star begins.")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 500f, 0);
    [Tooltip("Starting scale when spawned before animating to Vector3.one")]
    [SerializeField] private Vector3 startScale = Vector3.one * 1.5f;

    [Header("Prefabs & Slots")]
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private Transform[] emptyStarSlots;

    [Header("UI Elements")]
    [SerializeField] private GameObject nextLevelBtn;
    [SerializeField] private TextMeshProUGUI textTmp;

    public void GenerateStars(int earnedStars)
    {
        // Handle Next Level Button State
        UpdateButtonState(earnedStars > 0);

        // 1. Clear any previously generated child stars
        foreach (Transform slot in emptyStarSlots)
        {
            if (slot == null) continue;

            // Kill any active tweens on the slot's children before destroying
            foreach (Transform child in slot)
            {
                child.DOKill();
                Destroy(child.gameObject);
            }
        }

        // Clamp earnedStars to available slots limit
        int starsToAnimate = Mathf.Min(earnedStars, emptyStarSlots.Length);

        // 2. Build DOTween Sequence
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < starsToAnimate; i++)
        {
            int index = i; // Closure capture

            seq.AppendCallback(() =>
            {
                Transform targetSlot = emptyStarSlots[index];
                if (targetSlot == null) return;

                // Instantiate filled star directly inside the target empty star slot
                GameObject star = Instantiate(starPrefab, targetSlot);

                // Set initial transform states (Spawned far away with offset)
                RectTransform starRect = star.GetComponent<RectTransform>();
                if (starRect != null)
                {
                    starRect.anchoredPosition = spawnOffset;
                    starRect.anchorMin = new Vector2(0.5f, 0.5f);
                    starRect.anchorMax = new Vector2(0.5f, 0.5f);
                    starRect.localRotation = Quaternion.Euler(0, 0, 180f);
                    starRect.localScale = startScale;
                }
                else
                {
                    star.transform.localPosition = spawnOffset;
                    star.transform.localRotation = Quaternion.Euler(0, 0, 180f);
                    star.transform.localScale = startScale;
                }

                // Create inner concurrent tween group for position, rotation, and scale
                Sequence starTween = DOTween.Sequence();

                if (starRect != null)
                {
                    starTween.Join(starRect.DOAnchorPos(Vector2.zero, animationDuration).SetEase(moveEase));
                }
                else
                {
                    starTween.Join(star.transform.DOLocalMove(Vector3.zero, animationDuration).SetEase(moveEase));
                }

                starTween.Join(star.transform.DOLocalRotate(Vector3.zero, animationDuration).SetEase(moveEase));
                starTween.Join(star.transform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutQuad));
            });

            // Wait before firing the next star animation
            seq.AppendInterval(delayBetweenStars);
        }

        seq.Play();
    }

    private void UpdateButtonState(bool isEnabled)
    {
        if (nextLevelBtn == null) return;

        // Toggle interactivity
        Button button = nextLevelBtn.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = isEnabled;
        }

        // Toggle visual color (Gray when disabled, White/Normal when enabled)
        Image btnImage = nextLevelBtn.GetComponent<Image>();
        if (btnImage != null)
        {
            btnImage.color = isEnabled ? Color.white : Color.gray;
        }
    }

    public void SetScore(int totalCheaters, int selectedCheaters, int selectedNoneCheaters)
    {
        textTmp.text = "you got " + selectedCheaters + " out of " + totalCheaters + " cheaters and mistook " + selectedNoneCheaters + " none cheaters.";
    }
}