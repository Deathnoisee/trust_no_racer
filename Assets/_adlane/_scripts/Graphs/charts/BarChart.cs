using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BarChart : MonoBehaviour
{
    [SerializeField] private RectTransform chartArea;
    [SerializeField] private GameObject barPrefab; // a simple Image prefab (white, stretchable)
    [SerializeField] private TMP_Text axisLabelPrefab;
    [SerializeField] private int yAxisTickCount = 5;

    private List<ChartData> data;
    private List<Vector2> positions; // bottom-center of each bar

    public void SetData(List<ChartData> newData)
    {
        // Sort by numeric part of label (e.g., "0km" → 0)
        newData.Sort((a, b) =>
        {
            float aNum = ExtractNumeric(a.label);
            float bNum = ExtractNumeric(b.label);
            return aNum.CompareTo(bNum);
        });
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

        // Calculate min/max
        float minVal = float.MaxValue, maxVal = float.MinValue;
        foreach (var point in data)
        {
            minVal = Mathf.Min(minVal, point.value);
            maxVal = Mathf.Max(maxVal, point.value);
        }
        if (Mathf.Approximately(minVal, maxVal)) maxVal = minVal + 1f;

        Vector2 areaSize = chartArea.rect.size;
        float barWidth = areaSize.x / data.Count * 0.8f; // 80% of slot width
        float slotWidth = areaSize.x / data.Count;

        positions = new List<Vector2>();
        for (int i = 0; i < data.Count; i++)
        {
            float x = slotWidth * i + slotWidth * 0.5f; // center of slot
            float normalized = (data[i].value - minVal) / (maxVal - minVal);
            float y = normalized * areaSize.y;
            positions.Add(new Vector2(x, 0)); // bottom-center of bar
        }

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
            rt.anchoredPosition = new Vector2(-10f, y);
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
            rt.anchoredPosition = new Vector2(positions[i].x, -10f);
        }
    }
}