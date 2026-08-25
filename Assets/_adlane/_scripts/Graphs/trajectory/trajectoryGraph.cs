using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Strava-style trajectory path drawn as a single smooth mesh.
/// Feed it world-space points; they are converted to canvas space,
/// smoothed with Catmull-Rom, and animated as if being drawn.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class trajectoryGraph : MaskableGraphic
{
    [Header("Chart Area")]
    [SerializeField] private RectTransform chartArea;

    [Header("Path Styling")]
    [SerializeField] private float lineWidth = 6f;
    [SerializeField] private int smoothingSubdivisions = 6; // points added between each input point

    [Header("Start / End Markers")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private float markerSize = 14f;

    [Header("Animation")]
    [SerializeField] private float drawDuration = 1.5f;
    [SerializeField] private Ease drawEase = Ease.InOutQuad;

    [Header("World Conversion")]
    [SerializeField] private Camera worldCamera;

    // Full smoothed path in chartArea local space
    private readonly List<Vector2> pathPoints = new List<Vector2>();
    // Cumulative distance along the path (for even-progress drawing)
    private readonly List<float> cumulativeDistances = new List<float>();
    private float totalLength;

    // 0 → 1 draw progress, animated by DOTween
    private float drawProgress = 0f;

    private Transform markerParent;

    protected override void Awake()
    {
        base.Awake();
        AlignToChartArea();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        AlignToChartArea(); // in case chartArea was assigned after Awake
    }

    /// <summary>
    /// Makes this graphic's RectTransform exactly overlay chartArea,
    /// so its local mesh space == chartArea's local space.
    /// </summary>
    private void AlignToChartArea()
    {
        if (chartArea == null) return;

        // Make sure we're a direct child of chartArea
        if (transform.parent != chartArea)
            transform.SetParent(chartArea, false);

        RectTransform rt = rectTransform;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.localPosition = Vector3.zero;

        // Stretch to fill chartArea completely
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ---------------- Public API ----------------

    /// <summary>Feed raw world-space positions (e.g., recorded runner transforms).</summary>
    public void SetWorldPoints(List<Vector3> worldPositions)
    {
        // Same camera for both steps: it renders the world AND the UI (Screen Space - Camera)
        Camera cam = worldCamera != null ? worldCamera : Camera.main;
        if (cam == null)
        {
            Debug.LogError("trajectoryGraph: No camera assigned/found for world→canvas conversion.");
            return;
        }

        List<Vector2> localPoints = new List<Vector2>(worldPositions.Count);
        foreach (Vector3 wp in worldPositions)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(wp);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    chartArea, screenPos, cam, out Vector2 local)) // <-- pass cam, not null
            {
                localPoints.Add(local);
            }
        }

        BuildPath(localPoints);
    }

    /// <summary>Feed points already in chartArea local space.</summary>
    public void SetLocalPoints(List<Vector2> localPoints)
    {
        BuildPath(new List<Vector2>(localPoints));
    }

    /// <summary>Replays the draw animation.</summary>
    public void PlayDrawAnimation()
    {
        KillTweens();
        drawProgress = 0f;
        SetVerticesDirty();

        DOTween.To(() => drawProgress, x =>
        {
            drawProgress = x;
            SetVerticesDirty();
        }, 1f, drawDuration).SetEase(drawEase);

        ShowMarkers();
    }

    public void Clear()
    {
        KillTweens();
        pathPoints.Clear();
        cumulativeDistances.Clear();
        totalLength = 0f;
        drawProgress = 0f;
        HideMarkers();
        SetVerticesDirty();
    }

    // ---------------- Path building ----------------

    private void BuildPath(List<Vector2> rawPoints)
    {
        if (rawPoints == null || rawPoints.Count < 2) return;

        // Catmull-Rom smoothing through all input points
        pathPoints.Clear();
        int last = rawPoints.Count - 1;
        for (int i = 0; i < last; i++)
        {
            Vector2 p0 = rawPoints[Mathf.Max(i - 1, 0)];
            Vector2 p1 = rawPoints[i];
            Vector2 p2 = rawPoints[i + 1];
            Vector2 p3 = rawPoints[Mathf.Min(i + 2, last)];

            for (int s = 0; s < smoothingSubdivisions; s++)
            {
                float t = s / (float)smoothingSubdivisions;
                pathPoints.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }
        pathPoints.Add(rawPoints[last]);

        // Precompute cumulative distances so animation speed is constant
        cumulativeDistances.Clear();
        cumulativeDistances.Add(0f);
        totalLength = 0f;
        for (int i = 1; i < pathPoints.Count; i++)
        {
            totalLength += Vector2.Distance(pathPoints[i - 1], pathPoints[i]);
            cumulativeDistances.Add(totalLength);
        }

        PlayDrawAnimation();
    }

    private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    // ---------------- Mesh (single draw call) ----------------

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (pathPoints.Count < 2 || drawProgress <= 0f) return;

        float targetDistance = totalLength * drawProgress;

        // Collect only the visible portion of the path
        List<Vector2> visible = new List<Vector2>();
        visible.Add(pathPoints[0]);

        for (int i = 1; i < pathPoints.Count; i++)
        {
            if (cumulativeDistances[i] <= targetDistance)
            {
                visible.Add(pathPoints[i]);
            }
            else
            {
                // Partial segment: interpolate the exact tip of the growing line
                float prevDist = cumulativeDistances[i - 1];
                float segLen = cumulativeDistances[i] - prevDist;
                float t = segLen > 0f ? (targetDistance - prevDist) / segLen : 0f;
                visible.Add(Vector2.LerpUnclamped(pathPoints[i - 1], pathPoints[i], t));
                break;
            }
        }

        DrawLineStrip(visible, lineWidth, vh);
    }

    // Same technique as UIPipeLine.DrawLineStripMesh
    private void DrawLineStrip(List<Vector2> vertices, float width, VertexHelper vh)
    {
        if (vertices.Count < 2) return;

        float halfWidth = width * 0.5f;
        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 forward;
            if (i == 0)
                forward = (vertices[1] - vertices[0]).normalized;
            else if (i == vertices.Count - 1)
                forward = (vertices[i] - vertices[i - 1]).normalized;
            else
                forward = (vertices[i + 1] - vertices[i - 1]).normalized;

            Vector2 normal = new Vector2(-forward.y, forward.x) * halfWidth;

            vert.position = vertices[i] - normal;
            vh.AddVert(vert);

            vert.position = vertices[i] + normal;
            vh.AddVert(vert);

            if (i > 0)
            {
                int baseIndex = (i - 1) * 2;
                vh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
                vh.AddTriangle(baseIndex + 2, baseIndex + 1, baseIndex + 3);
            }
        }
    }

    // ---------------- Markers (start/end dots) ----------------

    private void ShowMarkers()
    {
        EnsureMarkerParent();
        ClearMarkers();

        CreateMarker(pathPoints[0], startColor, 0f);
        CreateMarker(pathPoints[pathPoints.Count - 1], endColor, drawDuration * 0.8f);
    }

    private void CreateMarker(Vector2 localPos, Color markerColor, float delay)
    {
        GameObject dot;
        if (dotPrefab != null)
        {
            dot = Instantiate(dotPrefab, markerParent);
        }
        else
        {
            dot = new GameObject("Marker", typeof(Image));
            dot.transform.SetParent(markerParent, false);
        }

        Image img = dot.GetComponent<Image>();
        img.color = markerColor;
        img.raycastTarget = false;

        RectTransform rt = (RectTransform)dot.transform;
        rt.sizeDelta = new Vector2(markerSize, markerSize);
        rt.localPosition = localPos;

        dot.transform.localScale = Vector3.zero;
        dot.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetDelay(delay);
    }

    private void EnsureMarkerParent()
    {
        if (markerParent != null) return;
        GameObject go = new GameObject("TrajectoryMarkers", typeof(RectTransform));
        go.transform.SetParent(chartArea, false);
        ((RectTransform)go.transform).localScale = Vector3.one;
        markerParent = go.transform;
    }

    private void ClearMarkers()
    {
        if (markerParent == null) return;
        for (int i = markerParent.childCount - 1; i >= 0; i--)
            Destroy(markerParent.GetChild(i).gameObject);
    }

    private void HideMarkers()
    {
        if (markerParent != null) markerParent.gameObject.SetActive(false);
    }

    private void KillTweens()
    {
        DOTween.Kill(this);
        if (markerParent != null)
            foreach (Transform child in markerParent)
                child.DOKill();
    }

    protected override void OnDestroy()
    {
        KillTweens();
        base.OnDestroy();
    }
}