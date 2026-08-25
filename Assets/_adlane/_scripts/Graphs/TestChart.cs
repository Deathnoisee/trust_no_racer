using UnityEngine;
using System.Collections.Generic;

public class TestChart : MonoBehaviour
{
    [SerializeField] private graphManager manager;

    private void Start()
    {
        // Simulate a racer's pace over 6 checkpoints
        List<ChartData> paceData = new List<ChartData>
        {
            new ChartData { label = "0km", value = 3.2f,  color = Color.green },
            new ChartData { label = "1km", value = 3.8f,  color = Color.green },
            new ChartData { label = "2km", value = 3.5f,  color = Color.green },
            new ChartData { label = "3km", value = 4.1f,  color = Color.green },
            new ChartData { label = "4km", value = 4.6f,  color = Color.green },
            new ChartData { label = "5km", value = 3.9f,  color = Color.green },
        };

        manager.ShowData(RaceDataType.Pace, paceData);
    }
}