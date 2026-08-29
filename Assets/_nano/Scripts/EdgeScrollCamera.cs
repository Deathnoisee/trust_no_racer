using UnityEngine;

public class EdgeScrollCamera : MonoBehaviour
{
    [Header("Edge Scroll")]
    public float edgeSize = 20f; // pixels from screen edge that trigger scrolling
    public float scrollSpeed = 10f;

    [Header("Bounds")]
    public Vector2 minBounds; // world-space min X/Y the camera can reach
    public Vector2 maxBounds; // world-space max X/Y the camera can reach

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        Vector3 moveDir = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;

        if (mousePos.x <= edgeSize)
            moveDir.x = -1f;
        else if (mousePos.x >= Screen.width - edgeSize)
            moveDir.x = 1f;

        if (mousePos.y <= edgeSize)
            moveDir.y = -1f;
        else if (mousePos.y >= Screen.height - edgeSize)
            moveDir.y = 1f;

        if (moveDir != Vector3.zero)
        {
            Vector3 newPos = transform.position + moveDir.normalized * scrollSpeed * Time.deltaTime;
            newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
            newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
    }
}
