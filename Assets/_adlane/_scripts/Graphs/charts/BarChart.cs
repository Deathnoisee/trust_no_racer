using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BarChart : MonoBehaviour
{
    [SerializeField] private RectTransform chartArea;
    [SerializeField] private GameObject barPrefab;
    [SerializeField] private TMP_Text axisLabelPrefab;
    [SerializeField] private int yAxisTickCount = 5;
    [SerializeField] private float yAxisMinOffset = 10f;
    [SerializeField] private float xLabelsOffset = 20f;
    [SerializeField] private float yLabelsOffset = 10f;

    [SerializeField] private float minVal = 0f;
    [SerializeField] private float maxVal = 100f;

    [Header("Offsets")]
    [SerializeField] private float minValuesOffset = 5f;
    [SerializeField] private float maxValuesOffset = 5f;

    private List<ChartData> data;
    private List<Vector2> positions;

    public void SetData(List<ChartData> newData, float minValue, float maxValue)
    {
        newData.Sort((a, b) =>
        {
            float aNum = ExtractNumeric(a.label);
            float bNum = ExtractNumeric(b.label);
            return aNum.CompareTo(bNum);
        });
        minVal = 0f;
        maxVal = maxValue + 5f;
        data = newData;
        Redraw();
    }

    private float ExtractNumeric(string label)
    {
        Match match = Regex.Match(label, @"(-?\d+\.?\d*)");
        if (match.Success)
            return float.Parse(match.Value);
        return 0f;
    }

    public void Redraw()
    {
        transform.DOKill();
        foreach (Transform child in chartArea) Destroy(child.gameObject);

        if (data == null || data.Count == 0) return;

        Vector2 areaSize = chartArea.rect.size;
        float barWidth = areaSize.x / data.Count * 0.8f;
        float slotWidth = areaSize.x / data.Count;

        positions = new List<Vector2>();
        for (int i = 0; i < data.Count; i++)
        {
            float x = slotWidth * i + slotWidth * 0.5f;
            float y = (data[i].value - minVal) / (maxVal - minVal) * areaSize.y;
            positions.Add(new Vector2(x, 0));
        }

        DrawGridLines(minVal, maxVal);

        // Draw axis labels
        DrawYAxisLabels(minVal, maxVal);
        DrawXAxisLabels();

        // Animate bars
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < data.Count; i++)
        {
            int index = i;
            seq.AppendCallback(() =>
            {
                GameObject bar = Instantiate(barPrefab, chartArea);
                RectTransform rt = bar.GetComponent<RectTransform>();
                Image img = bar.GetComponent<Image>();
                img.color = data[index].color;

                // Position and size
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0f); // bottom-center
                rt.anchoredPosition = positions[index];
                rt.sizeDelta = new Vector2(barWidth, 0f); // start height 0

                // Animate height
                float targetHeight = (data[index].value - minVal) / (maxVal - minVal) * areaSize.y;
                rt.DOSizeDelta(new Vector2(barWidth, targetHeight), 0.4f).SetEase(Ease.OutQuad);
            });
            seq.AppendInterval(0.15f);
        }

        // Fade in axis labels after bars
        seq.AppendCallback(() =>
        {
            foreach (Transform child in chartArea)
            {
                TMP_Text tmp = child.GetComponent<TMP_Text>();
                if (tmp != null)
                {
                    tmp.alpha = 0f;
                    tmp.DOFade(1f, 0.3f);
                }
            }
        });

        seq.Play();
    }

    private void DrawYAxisLabels(float minVal, float maxVal)
    {
        if (axisLabelPrefab == null) return;

        float step = (maxVal - minVal) / (yAxisTickCount - 1);
        float areaHeight = chartArea.rect.height;

        for (int i = 0; i < yAxisTickCount; i++)
        {
            float value = minVal + i * step;
            float normalized = (value - minVal) / (maxVal - minVal);
            float y = normalized * areaHeight;

            TMP_Text label = Instantiate(axisLabelPrefab, chartArea);
            label.text = value.ToString("F1");
            label.alignment = TextAlignmentOptions.Right;

            RectTransform rt = label.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-yLabelsOffset, y);
        }
    }

    private void DrawXAxisLabels()
    {
        if (axisLabelPrefab == null || positions == null) return;

        for (int i = 0; i < positions.Count; i++)
        {
            TMP_Text label = Instantiate(axisLabelPrefab, chartArea);
            label.text = data[i].label;
            label.alignment = TextAlignmentOptions.Center;

            RectTransform rt = label.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(positions[i].x, -xLabelsOffset);
        }
    }

    private void DrawGridLines(float minVal, float maxVal)
    {
        Color gridColor = new Color(1, 1, 1, 0.05f);
        float areaWidth = chartArea.rect.width;
        float areaHeight = chartArea.rect.height;

        // Horizontal
        float step = (maxVal - minVal) / (yAxisTickCount - 1);
        for (int i = 0; i < yAxisTickCount; i++)
        {
            float value = minVal + i * step;
            float normalized = (value - minVal) / (maxVal - minVal);
            float y = normalized * areaHeight;
            DrawLine(new Vector2(0, y), new Vector2(areaWidth, y), gridColor, 1f);
        }

        // Vertical (aligned with data points)
        for (int i = 0; i < positions.Count; i++)
        {
            DrawLine(positions[i], new Vector2(positions[i].x, areaHeight), gridColor, 1f);
        }
    }

    private GameObject DrawLine(Vector2 from, Vector2 to, Color color, float thickness)
    {
        GameObject lineObj = new GameObject("LineSegment", typeof(Image));
        lineObj.transform.SetParent(chartArea, false);
        lineObj.GetComponent<Image>().color = color;

        RectTransform rt = lineObj.GetComponent<RectTransform>();
        Vector2 dir = to - from;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(distance, thickness); // full size – will be overridden by tween
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.anchoredPosition = from;
        rt.localEulerAngles = new Vector3(0, 0, angle);
        return lineObj;
    }
}