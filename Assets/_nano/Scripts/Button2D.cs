using UnityEngine;

public class Button2D : MonoBehaviour
{
   public enum ButtonState { Normal, Highlighted, Pressed }

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite highlightedSprite;
    public Sprite pressedSprite;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onClick;

    private SpriteRenderer sr;
    private ButtonState currentState = ButtonState.Normal;
    private bool isPointerOver = false;
    private bool isPressed = false;
    private Camera cam;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        SetState(ButtonState.Normal);
    }

    void Update()
    {
        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
        bool overThisButton = (hit != null && hit.gameObject == gameObject);

        // entered / exited
        if (overThisButton && !isPointerOver)
        {
            isPointerOver = true;
            if (!isPressed) SetState(ButtonState.Highlighted);
        }
        else if (!overThisButton && isPointerOver)
        {
            isPointerOver = false;
            if (!isPressed) SetState(ButtonState.Normal);
        }

        // pressed / released
        if (overThisButton && Input.GetMouseButtonDown(0))
        {
            isPressed = true;
            SetState(ButtonState.Pressed);
        }

        if (isPressed && Input.GetMouseButtonUp(0))
        {
            isPressed = false;

            if (overThisButton)
            {
                SetState(ButtonState.Highlighted);
                onClick?.Invoke();
            }
            else
            {
                SetState(ButtonState.Normal);
            }
        }
    }

    void SetState(ButtonState state)
    {
        currentState = state;
        switch (state)
        {
            case ButtonState.Normal:
                sr.sprite = normalSprite;
                break;
            case ButtonState.Highlighted:
                sr.sprite = highlightedSprite;
                break;
            case ButtonState.Pressed:
                sr.sprite = pressedSprite;
                break;
        }
    }
}
