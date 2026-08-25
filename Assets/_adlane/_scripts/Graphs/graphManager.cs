using UnityEngine;
using System.Collections.Generic;

public enum RaceDataType { Pace, HeartRate, BloodTest, UrineTest }

public class graphManager : MonoBehaviour
{
    [SerializeField] private LineChart lineChart;
    [SerializeField] private BarChart barChart;
    // [SerializeField] private PieChart pieChart;


    public void ShowData(RaceDataType type, List<ChartData> data)
    {
        switch (type)
        {
            case RaceDataType.Pace:
                ShowLineChart(data);
                break;
            case RaceDataType.HeartRate:
                ShowBarChart(data);
                break;
            case RaceDataType.BloodTest:
            case RaceDataType.UrineTest:
                ShowBarChart(data);
                break;
        }
    }

    private void ShowLineChart(List<ChartData> data)
    {
        lineChart.gameObject.SetActive(true);
        lineChart.SetData(data);
    }

    private void ShowBarChart(List<ChartData> data)
    {
        barChart.gameObject.SetActive(true);
        barChart.SetData(data);
    }
}

[System.Serializable]
public class ChartData
{
    public string label;
    public float value;
    public Color color = Color.white;
}