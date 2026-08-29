using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class DrugsManager : MonoBehaviour
{
    public List<solution> solutions = new List<solution>();
    public blood bloodTarget;

    private void OnEnable()
    {
        if (RunnersGenerator.instance != null)
        {
            RunnersGenerator.instance.analysePlayer += ChangeTarget;
            RunnersGenerator.instance.tubesButton += SetupCorrectSolution;
        }
    }

    private void OnDisable()
    {
        if (RunnersGenerator.instance != null)
        {
            RunnersGenerator.instance.analysePlayer -= ChangeTarget;
            RunnersGenerator.instance.tubesButton -= SetupCorrectSolution;
        }
    }

    private void ChangeTarget(RunnerData runnerData)
    {
        SetupCorrectSolution(runnerData);
    }

    private void SetupCorrectSolution(RunnerData runnerData)
    {
        if (bloodTarget == null) return;

        // If this runner already had a correct drug test, show green sprite and hide all solutions
        if (runnerData.drugTestCorrect)
        {
            bloodTarget.GetComponent<Image>().sprite = bloodTarget.GreenSprite;
            HideAllSolutions();
            return;
        }

        // If this runner already attempted a test (even incorrect), keep original sprite and hide all solutions
        if (runnerData.earlyBloodTestDone || runnerData.midBloodTestDone || runnerData.lateBloodTestDone)
        {
            bloodTarget.GetComponent<Image>().sprite = bloodTarget.originalSprite;
            HideAllSolutions();
            return;
        }

        // Fresh runner: reset sprite and show only unused solutions
        bloodTarget.GetComponent<Image>().sprite = bloodTarget.originalSprite;

        foreach (var sol in solutions)
        {
            bool phaseDone = (sol.racePhaseSolution == RacePhase.Early && runnerData.earlyBloodTestDone) ||
                             (sol.racePhaseSolution == RacePhase.Mid && runnerData.midBloodTestDone) ||
                             (sol.racePhaseSolution == RacePhase.Late && runnerData.lateBloodTestDone);

            sol.gameObject.SetActive(!phaseDone);

            // Correctness
            sol.isSolutionCorrect = (runnerData.cheatType == CheatType.SpeedBoost &&
                                     runnerData.CheatTimePhase == sol.racePhaseSolution);
        }
    }

    private void HideAllSolutions()
    {
        foreach (var sol in solutions)
        {
            sol.gameObject.SetActive(false);
        }
    }
}

