using DG.Tweening;
using UnityEngine;


public class StarsGenerator : MonoBehaviour
{
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private Transform starsParent;
    [SerializeField] private int totalStars = 3;
    [SerializeField] private float popDuration = 0.5f;
    [SerializeField] private float delayBetweenStars = 0.2f;
    [SerializeField] private Ease popEase = Ease.OutBounce;

    public void GenerateStars(int earnedStars)
    {
        // Clear existing stars (if any)
        foreach (Transform child in starsParent)
        {
            Destroy(child.gameObject);
        }

        // Create a sequence for the animation
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < totalStars; i++)
        {
            int index = i; // capture for closure
            seq.AppendCallback(() =>
            {
                GameObject star = Instantiate(starPrefab, starsParent);

                // Start invisible and small
                star.transform.localScale = Vector3.zero;

                // Determine if star should be active (earned) or inactive (empty)
                bool isEarned = index < earnedStars;

                if (isEarned)
                {
                    // Pop-in animation for earned star
                    star.transform.DOScale(Vector3.one, popDuration)
                        .SetEase(popEase)
                        .SetDelay(0.1f); // subtle delay per star
                }
                else
                {
                    // For unearned stars, just show a smaller scale or dim
                    star.transform.DOScale(Vector3.one * 0.6f, popDuration * 0.5f)
                        .SetEase(Ease.OutQuad);
                }
            });

            seq.AppendInterval(delayBetweenStars);
        }

        seq.Play();
    }
}