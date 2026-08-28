using UnityEngine;
using System.Collections.Generic;

public enum RaceDataType { Pace, HeartRate, BloodTest, UrineTest, Trajectory }

public class graphManager : MonoBehaviour
{
    [SerializeField] private LineChart lineChart;
    [SerializeField] private BarChart barChart;
    [SerializeField] private trajectoryGraph trajectoryChart;
    [SerializeField] private float valueNano = 100f;
    private float minValue = -1;
    private float maxValue = -1;

    // [SerializeField] private PieChart pieChart;

    public void Start()
    {
        lineChart.gameObject.SetActive(false);
        barChart.gameObject.SetActive(false);
        // trajectoryChart.gameObject.SetActive(false);
        Time.timeScale *= 10f;

        if (RunnersGenerator.instance != null)
        {
            RunnersGenerator.instance.analysePlayer += HandleAnalysePlayer;
        }
    }

    private void OnDisable()
    {
        if (RunnersGenerator.instance != null)
        {
            RunnersGenerator.instance.analysePlayer -= HandleAnalysePlayer;
        }
    }

    private void HandleAnalysePlayer(RunnerData runnerData)
    {
        Debug.LogWarning("Received runner data for analysis: ");
        if (minValue == -1f && maxValue == -1f)
        {
            if (RunnersGenerator.instance == null)
            {
                Debug.LogError("RunnersGenerator instance is null. Cannot analyze player data.");
                return;
            }
            foreach (var runner in RunnersGenerator.instance.currentRunners)
            {
                if (runner == null || runner.kmSplits == null)
                {
                    if (runner == null)
                    {
                        Debug.LogWarning("Found null runner in currentRunners. Skipping.");
                    }
                    else
                    {
                        Debug.LogWarning($"Found null kmSplits for runner. Skipping.");
                    }
                    continue;
                }
                foreach (var split in runner.kmSplits)
                {
                    float paceKmh = split.paceKmh / valueNano;
                    if (minValue == -1f || paceKmh < minValue)
                    {
                        minValue = Mathf.Max(0, paceKmh);
                    }
                    if (maxValue == -1f || paceKmh > maxValue)
                    {
                        maxValue = paceKmh;
                    }
                }
                print($"minValue: {minValue}, maxValue: {maxValue}");
            }
        }
        ShowData(RaceDataType.Pace, runnerData);
    }

    public void ShowData(RaceDataType type, RunnerData incomingData)
    {
        List<ChartData> data = convertData(type, incomingData);
        switch (type)
        {
            case RaceDataType.Pace:
                ShowBarChart(data);
                break;
            case RaceDataType.HeartRate:
                ShowBarChart(data);
                break;
            case RaceDataType.BloodTest:
            case RaceDataType.UrineTest:
                ShowBarChart(data);
                break;
            case RaceDataType.Trajectory:
                ShowLineChart(data);
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
        if (minValue == -1f || maxValue == -1f)
        {
            foreach (var chartData in data)
            {
                if (minValue == -1f)
                {
                    Debug.LogWarning("minValue is -1, setting to 0f");
                    minValue = 0f;
                }
                if (maxValue == -1f)
                {
                    Debug.LogWarning("maxValue is -1, setting to 100f");
                    maxValue = 100f;
                }
            }
        }
        barChart.SetData(data, minValue, maxValue);
    }
    public void ShowTrajectoryChart(List<TrajectoryPoint> data)
    {
        trajectoryChart.gameObject.SetActive(true);
        List<Vector3> worldPoints = new List<Vector3>();
        foreach (var point in data)
        {
            worldPoints.Add(new Vector3(point.position.x, point.position.y, point.position.z));
        }

        trajectoryChart.SetWorldPoints(worldPoints);
    }
    private List<ChartData> convertData(RaceDataType type, RunnerData data)
    {
        List<ChartData> chartData = new List<ChartData>();
        switch (type)
        {
            case RaceDataType.Pace:
                if (data.kmSplits == null)
                {
                    Debug.LogWarning("kmSplits is null in RunnerData. Returning empty chart data.");
                    return chartData;
                }
                foreach (var split in data.kmSplits)
                {
                    if (split == null)
                    {
                        Debug.LogWarning("Found null split in kmSplits. Skipping.");

                        continue;
                    }
                    chartData.Add(new ChartData
                    {
                        label = split.kmIndex.ToString(),
                        value = split.paceKmh / valueNano,
                    });
                }
                break;
        }
        return chartData;
    }
}

[System.Serializable]
public class ChartData
{
    public string label;
    public float value;
    public Color color = Color.white;
}