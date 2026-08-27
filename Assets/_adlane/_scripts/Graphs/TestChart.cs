using UnityEngine;
using System.Collections.Generic;

public class TestChart : MonoBehaviour
{
    [SerializeField] private graphManager manager;

    private void Start()
    {
        List<ChartData> paceData = new List<ChartData>
        {
            new ChartData { label = "0km", value = 3.2f,  color = Color.green },
            new ChartData { label = "1km", value = 3.8f,  color = Color.green },
            new ChartData { label = "2km", value = 3.5f,  color = Color.green },
            new ChartData { label = "3km", value = 4.1f,  color = Color.green },
            new ChartData { label = "4km", value = 4.6f,  color = Color.green },
            new ChartData { label = "5km", value = 3.9f,  color = Color.green },
        };
        List<ChartData> heartRateData = new List<ChartData>
        {
            new ChartData { label = "0km", value = 120f, color = Color.red },
            new ChartData { label = "1km", value = 130f, color = Color.red },
            new ChartData { label = "2km", value = 125f, color = Color.red },
            new ChartData { label = "3km", value = 140f, color = Color.red },
            new ChartData { label = "4km", value = 150f, color = Color.red },
            new ChartData { label = "5km", value = 135f, color = Color.red },
        };

        manager.ShowData(RaceDataType.Pace, paceData);
        manager.ShowData(RaceDataType.HeartRate, heartRateData);

        List<Vector3> worldPoints = new List<Vector3>();
        for (int i = 0; i < 40; i++)
        {
            float t = i / 39f;
            worldPoints.Add(new Vector3(
                t * 80f,                          // runs "forward" along the track
                Mathf.Sin(t * Mathf.PI * 3f) * 10f + Mathf.Cos(t * Mathf.PI * 7f) * 3f,
                0f));
        }
        manager.ShowTrajectoryChart(worldPoints);
    }
}