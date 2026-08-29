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
    [SerializeField] private float lineWidth = 10f;
    [SerializeField] private int smoothingSubdivisions = 6;
    [Header("Start / End Markers")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private float markerSize = 22f;

    [Header("Animation")]
    [SerializeField] private float drawDuration = 1.5f;
    [SerializeField] private Ease drawEase = Ease.InOutQuad;

    [Header("World Conversion")]
    [SerializeField] private Camera worldCamera;

    private readonly List<Vector2> pathPoints = new List<Vector2>();
    private readonly List<float> cumulativeDistances = new List<float>();
    private float totalLength;
    private float drawProgress = 0f;
    private readonly List<GameObject> spawnedMarkers = new List<GameObject>();
    [SerializeField] private Color coloooor = Color.white;

    private Tween drawTween; // <-- add this field

    protected override void Awake()
    {
        // base.Awake();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        SetupRectTransform();
        chartArea.gameObject.SetActive(true);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        chartArea.gameObject.SetActive(false);
    }

    private void SetupRectTransform()
    {
        if (chartArea == null) return;

        // Ensure we are a child of chartArea so we render inside it
        if (transform.parent != chartArea)
            transform.SetParent(chartArea, false);

        // Configure rect to stretch and fill chartArea perfectly using Unity UI anchors
        RectTransform rt = rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0f, 0f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.localPosition = Vector3.zero;
    }

    public void SetWorldPoints(List<Vector3> worldPositions)
    {
        Clear(); // <-- reset everything before building new path
        SetupRectTransform();

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
                    chartArea, screenPos, cam, out Vector2 local))
            {
                localPoints.Add(local);
            }
        }

        BuildPath(localPoints);
    }

    public void SetLocalPoints(List<Vector2> localPoints)
    {
        Clear(); // <-- reset everything before building new path
        SetupRectTransform();
        BuildPath(new List<Vector2>(localPoints));
    }

    public void PlayDrawAnimation()
    {
        KillTweens(); // kills drawTween and marker tweens
        drawProgress = 0f;
        SetVerticesDirty();

        drawTween = DOTween.To(() => drawProgress, x =>
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

    private void BuildPath(List<Vector2> rawPoints)
    {
        if (rawPoints == null || rawPoints.Count < 2) return;

        List<Vector2> fittedPoints = FitPointsToChartArea(rawPoints);

        pathPoints.Clear();
        int last = fittedPoints.Count - 1;
        for (int i = 0; i < last; i++)
        {
            Vector2 p0 = fittedPoints[Mathf.Max(i - 1, 0)];
            Vector2 p1 = fittedPoints[i];
            Vector2 p2 = fittedPoints[i + 1];
            Vector2 p3 = fittedPoints[Mathf.Min(i + 2, last)];

            for (int s = 0; s < smoothingSubdivisions; s++)
            {
                float t = s / (float)smoothingSubdivisions;
                pathPoints.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }
        pathPoints.Add(fittedPoints[last]);

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

    private List<Vector2> FitPointsToChartArea(List<Vector2> points)
    {
        if (points.Count == 0) return points;

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var p in points)
        {
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }

        float rangeX = maxX - minX;
        float rangeY = maxY - minY;

        if (Mathf.Approximately(rangeX, 0f)) rangeX = 1f;
        if (Mathf.Approximately(rangeY, 0f)) rangeY = 1f;

        // Force layout update to ensure rect size is valid
        Canvas.ForceUpdateCanvases();
        Vector2 areaSize = chartArea != null ? chartArea.rect.size : new Vector2(200f, 200f);

        float margin = 40f;
        float usableWidth = Mathf.Max(areaSize.x - margin * 2f, 10f);
        float usableHeight = Mathf.Max(areaSize.y - margin * 2f, 10f);

        float scale = Mathf.Min(usableWidth / rangeX, usableHeight / rangeY);

        List<Vector2> fitted = new List<Vector2>(points.Count);

        foreach (var p in points)
        {
            float x = margin + (p.x - minX) * scale;
            float y = margin + (p.y - minY) * scale;
            fitted.Add(new Vector2(x, y));
        }

        return fitted;
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

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (pathPoints.Count < 2 || drawProgress <= 0f) return;

        float targetDistance = totalLength * drawProgress;

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
                float prevDist = cumulativeDistances[i - 1];
                float segLen = cumulativeDistances[i] - prevDist;
                float t = segLen > 0f ? (targetDistance - prevDist) / segLen : 0f;
                visible.Add(Vector2.LerpUnclamped(pathPoints[i - 1], pathPoints[i], t));
                break;
            }
        }

        DrawLineStrip(visible, lineWidth, vh);
    }

    private void DrawLineStrip(List<Vector2> vertices, float width, VertexHelper vh)
    {
        if (vertices.Count < 2) return;

        float halfWidth = width * 0.5f;
        UIVertex vert = UIVertex.simpleVert;
        vert.color = coloooor;

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

    private void ShowMarkers()
    {
        ClearMarkers();
        CreateMarker(pathPoints[0], startColor, 0f);
        CreateMarker(pathPoints[pathPoints.Count - 1], endColor, drawDuration * 0.8f);
    }

    private void CreateMarker(Vector2 localPos, Color markerColor, float delay)
    {
        GameObject dot;
        if (dotPrefab != null)
        {
            dot = Instantiate(dotPrefab, chartArea);
        }
        else
        {
            dot = new GameObject("Marker", typeof(Image));
            dot.transform.SetParent(chartArea, false);
        }

        Image img = dot.GetComponent<Image>();
        img.color = markerColor;
        img.raycastTarget = false;

        RectTransform rt = (RectTransform)dot.transform;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.sizeDelta = new Vector2(markerSize, markerSize);
        rt.localPosition = localPos;

        dot.transform.localScale = Vector3.zero;
        dot.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetDelay(delay);

        spawnedMarkers.Add(dot);
    }

    private void ClearMarkers()
    {
        foreach (var marker in spawnedMarkers)
        {
            if (marker != null)
            {
                marker.transform.DOKill();
                Destroy(marker);
            }
        }
        spawnedMarkers.Clear();
    }

    private void HideMarkers()
    {
        ClearMarkers();
    }

    private void KillTweens()
    {
        if (drawTween != null)
        {
            drawTween.Kill();
            drawTween = null;
        }
        foreach (var marker in spawnedMarkers)
        {
            if (marker != null)
                marker.transform.DOKill();
        }
    }

    protected override void OnDestroy()
    {
        KillTweens();
        base.OnDestroy();
    }
}