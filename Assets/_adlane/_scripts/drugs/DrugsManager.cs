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


    private void SetupCorrectSolution(RunnerData runnerData)
    {
        bloodTarget.ResetBlood();
        changedTarget = false;
        foreach (var sol in solutions)
        {
            RunnerData currentRunner = RunnersGenerator.instance.currentRunners[RunnersGenerator.instance.currentRunnerIndex];
            //Appearence
            if (currentRunner.earlyBloodTestDone && sol.racePhaseSolution == RacePhase.Early)
            {
                sol.gameObject.SetActive(false);
                continue;
            }
            else if (currentRunner.midBloodTestDone && sol.racePhaseSolution == RacePhase.Mid)
            {
                sol.gameObject.SetActive(false);
                continue;
            }
            else if (currentRunner.lateBloodTestDone && sol.racePhaseSolution == RacePhase.Late)
            {
                sol.gameObject.SetActive(false);
                continue;
            }
            else
            {
                sol.gameObject.SetActive(true);
            }

            //Correctness
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