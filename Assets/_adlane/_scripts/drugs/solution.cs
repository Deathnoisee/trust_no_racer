using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class solution : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RacePhase racePhaseSolution;
    public bool isSolutionCorrect = false;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        rectTransform.DOKill();

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;

        transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        bool droppedOnTarget = false;
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            blood bloodTarget = eventData.pointerCurrentRaycast.gameObject.GetComponent<blood>();
            if (bloodTarget != null)
            {
                bloodTarget.ReceiveSolution(this);
                droppedOnTarget = true;
            }
            transform.SetParent(originalParent, true);
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            rectTransform.DOAnchorPos(originalPosition, 0.3f).SetEase(Ease.OutQuad);
        }

        if (!droppedOnTarget)
        {
            transform.SetParent(originalParent, true);
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            rectTransform.DOAnchorPos(originalPosition, 0.3f).SetEase(Ease.OutQuad);
        }
    }
}
