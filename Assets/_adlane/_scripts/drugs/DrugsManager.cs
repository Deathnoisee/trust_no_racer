using UnityEngine;
using System.Collections.Generic;
public class DrugsManager : MonoBehaviour
{
    public List<solution> solutions = new List<solution>();
    public void Start()
    {
        RunnersGenerator.instance.analysePlayer += SetupCorrectSolution;
    }
    public void OnDisable()
    {
        RunnersGenerator.instance.analysePlayer -= SetupCorrectSolution;
    }

    private void SetupCorrectSolution(RunnerData runnerData)
    {
        foreach (var sol in solutions)
        {
            if (runnerData.cheatType == CheatType.SpeedBoost && runnerData.CheatTimePhase == sol.racePhaseSolution)
            {
                sol.isSolutionCorrect = true;
            }
            else
            {
                sol.isSolutionCorrect = false;
            }
        }
    }
}