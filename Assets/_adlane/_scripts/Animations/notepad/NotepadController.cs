using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class NotepadController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform notepadRect;
    [SerializeField] private float shownY = 0f;
    [SerializeField] private float hiddenY = -320;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private NotepadManager notepadManager;

    void Start()
    {
        notepadManager = gameObject.GetComponent<NotepadManager>();
        if (notepadManager == null)
        {
            Debug.LogError("NotepadManager component not found on the GameObject.");
        }
        notepadRect = GetComponent<RectTransform>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) && !notepadManager.NotepadInputField.isFocused)
        {
            ToggleNotepad();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleNotepad();
    }

    public void ToggleNotepad()
    {
        if (notepadManager == null) return;

        bool willBeOpen = !notepadManager.IsOpen;
        float targetY = willBeOpen ? shownY : hiddenY;

        notepadManager.SetOpenState(willBeOpen);

        notepadRect.DOKill();
        notepadRect.DOAnchorPosY(targetY, animationDuration)
            .SetEase(Ease.OutBack);
    }
}