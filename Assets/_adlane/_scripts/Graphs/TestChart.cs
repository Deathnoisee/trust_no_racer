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

        // manager.ShowData(RaceDataType.Pace, paceData);
        // manager.ShowData(RaceDataType.HeartRate, heartRateData);

        // if (RunnersGenerator.instance != null)
        // {
        //     RunnersGenerator.instance.analysePlayer += test;
        // }
    }
    // private void OnDisable()
    // {
    //     if (RunnersGenerator.instance != null)
    //     {
    //         RunnersGenerator.instance.analysePlayer -= test;
    //     }
    // }


    // public void test(RunnerData data)
    // {
        
    //     manager.ShowTrajectoryChart(RunnersGenerator.instance.currentRunners[RunnersGenerator.instance.currentRunnerIndex].trajectory);
    // }
}