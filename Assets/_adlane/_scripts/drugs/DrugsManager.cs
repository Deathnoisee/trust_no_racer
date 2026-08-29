using UnityEngine;
using System.Collections.Generic;
public class DrugsManager : MonoBehaviour
{
    public List<solution> solutions = new List<solution>();
    public blood bloodTarget;
    private bool changedTarget = false;
    public void Start()
    {
        RunnersGenerator.instance.analysePlayer += ChangeTarget;
        RunnersGenerator.instance.tubesButton += SetupCorrectSolution;
    }
    public void OnEnable()
    {
        RunnersGenerator.instance.analysePlayer += ChangeTarget;
        RunnersGenerator.instance.tubesButton += SetupCorrectSolution;
    }
    public void OnDisable()
    {
        RunnersGenerator.instance.analysePlayer -= ChangeTarget;
        RunnersGenerator.instance.tubesButton -= SetupCorrectSolution;
    }
    private void ChangeTarget(RunnerData runnerData)
    {
        changedTarget = true;
    }

    private void ShowTrajectory(RunnerData runnerData)
    {
        foreach (var sol in solutions)
        {
            sol.gameObject.SetActive(false);
        }
    }
    private void SetupCorrectSolution(RunnerData runnerData)
    {
        bloodTarget.ResetBlood();
        changedTarget = false;
        foreach (var sol in solutions)
        {
            if (runnerData.cheatType == CheatType.SpeedBoost && runnerData.CheatTimePhase == sol.racePhaseSolution)
            {
                sol.gameObject.SetActive(true);
                sol.isSolutionCorrect = true;
            }
            else
            {
                sol.gameObject.SetActive(true);
                sol.isSolutionCorrect = false;
            }
        }
    }

}