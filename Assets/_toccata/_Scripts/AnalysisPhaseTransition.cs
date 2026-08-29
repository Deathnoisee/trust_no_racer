using UnityEngine;
using DG.Tweening;
using System.Collections;

public class AnalysisPhaseTransition : MonoBehaviour
{
    [Header("UI Target")]
    [SerializeField] private RectTransform canvasPanel;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private float startOffscreenY = 1100f; // Height to drop from


    private Vector2 initialPosition;
    private void Start()
    {
        GameManager.instance.raceManager.OnRaceEnded += SlideDownWithBounce;
    }
    private void OnDisable()
    {
        GameManager.instance.raceManager.OnRaceEnded -= SlideDownWithBounce;
    }
    private void Awake()
    {

        if (canvasPanel != null)
        {
            // Store the target/initial position set in the Unity Inspector
            initialPosition = canvasPanel.anchoredPosition;
        }
        canvasPanel.gameObject.SetActive(false);
    }

    [ContextMenu("Show Analysis Phase")]
    public void SlideDownWithBounce()
    {
        print("GOT RACE ENDED EVENT NOOOOOOOOOOOW");
        StartCoroutine(DoAnimation());
    }

    IEnumerator DoAnimation()
    {
        if (canvasPanel == null) yield return null;
        yield return new WaitForSeconds(1f);
        // Start offscreen directly above its initial position
        canvasPanel.anchoredPosition = initialPosition + new Vector2(0, startOffscreenY);
        canvasPanel.gameObject.SetActive(true);

        ListImporter.instance.loadListNames();
        RunnersGenerator.instance.InitializePhase();

        yield return new WaitForSeconds(0.5f);


        // Animate to initial position with a bounce effect
        canvasPanel.DOAnchorPos(initialPosition, duration)
            .SetEase(Ease.OutBounce);
    }
    IEnumerator SlideDownAnimation()
    {
        if (canvasPanel == null) yield break;

        // Wait a moment before starting
        yield return new WaitForSeconds(0.5f);

        // Start offscreen above its initial position
        canvasPanel.anchoredPosition = initialPosition + new Vector2(0, startOffscreenY);
        canvasPanel.gameObject.SetActive(true);

        // Optional: load data before sliding in
        ListImporter.instance.loadListNames();
        RunnersGenerator.instance.InitializePhase();

        yield return new WaitForSeconds(0.5f);

        // Smooth slide down to the target position
        canvasPanel.DOAnchorPos(initialPosition, duration)
            .SetEase(Ease.OutCubic);
    }
    public void NextButtonClicked()
    {
        // Start the slide down animation
        StartCoroutine(SlideDownAnimation());
    }

}