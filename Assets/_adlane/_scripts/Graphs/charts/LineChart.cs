using DG.Tweening; // <-- add this
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LineChart : MonoBehaviour
{
    [SerializeField] private RectTransform chartArea;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private float lineThickness = 4f;
    [SerializeField] private TMP_Text axisLabelPrefab;
    [SerializeField] private int yAxisTickCount = 5;

    private List<ChartData> data;
    private List<Vector2> positions;

    public void SetData(List<ChartData> newData)
    {
        // Sort data by the numeric part of the label (e.g., "0km" → 0)
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

        float minVal = float.MaxValue, maxVal = float.MinValue;
        foreach (var point in data)
        {
            minVal = Mathf.Min(minVal, point.value);
            maxVal = Mathf.Max(maxVal, point.value);
        }
        if (Mathf.Approximately(minVal, maxVal)) maxVal = minVal + 1f;

        Vector2 areaSize = chartArea.rect.size;
        positions = new List<Vector2>();
        for (int i = 0; i < data.Count; i++)
        {
            float t = data.Count == 1 ? 0.5f : (float)i / (data.Count - 1);
            float x = t * areaSize.x;

            float normalized = (data[i].value - minVal) / (maxVal - minVal);
            float y = normalized * areaSize.y;

            positions.Add(new Vector2(x, y));
        }


        Sequence seq = DOTween.Sequence();

        //  Axis labels fade in
        seq.AppendCallback(() =>
        {
            DrawYAxisLabels(minVal, maxVal);
            DrawXAxisLabels();
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


        //  Animate dots appearing 
        for (int i = 0; i < positions.Count; i++)
        {
            int index = i;
            seq.AppendCallback(() =>
            {
                GameObject dot = DrawDot(positions[index], data[index].color);
                if (dot != null)
                {
                    dot.transform.localScale = Vector3.zero;
                    dot.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
                }
            });
            seq.AppendInterval(0.05f);
        }



        // Draw lines with width animation, segment by segment
        for (int i = 0; i < positions.Count - 1; i++)
        {
            int index = i;
            seq.AppendCallback(() =>
            {
                GameObject line = DrawLine(positions[index], positions[index + 1], data[index].color, lineThickness);
                if (line != null)
                {
                    // Set initial width to 0
                    RectTransform rt = line.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(0, lineThickness);
                    // Tween width to full distance
                    float distance = Vector2.Distance(positions[index], positions[index + 1]);
                    rt.DOSizeDelta(new Vector2(distance, lineThickness), 0.3f).SetEase(Ease.OutQuad);
                }
            });
            seq.AppendInterval(0.3f);
        }

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

    private GameObject DrawDot(Vector2 position, Color color)
    {
        if (dotPrefab == null) return null;
        GameObject dot = Instantiate(dotPrefab, chartArea);
        dot.transform.localPosition = position;
        dot.GetComponent<Image>().color = color;
        return dot;
    }

}