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

        // If this runner already had a correct drug test, show green sprite and skip reset
        if (runnerData.drugTestCorrect)
        {
            bloodTarget.GetComponent<Image>().sprite = bloodTarget.GreenSprite; // you need to expose greenSprite in blood.cs
            return;
        }

        // Reset to original sprite for a fresh test
        bloodTarget.GetComponent<Image>().sprite = bloodTarget.originalSprite;

        foreach (var sol in solutions)
        {
            // Use the passed runnerData directly
            if (runnerData.earlyBloodTestDone && sol.racePhaseSolution == RacePhase.Early)
            {
                sol.gameObject.SetActive(false);
            }
            else if (runnerData.midBloodTestDone && sol.racePhaseSolution == RacePhase.Mid)
            {
                sol.gameObject.SetActive(false);
            }
            else if (runnerData.lateBloodTestDone && sol.racePhaseSolution == RacePhase.Late)
            {
                sol.gameObject.SetActive(false);
            }
            else
            {
                sol.gameObject.SetActive(true);
            }

            // Correctness
            sol.isSolutionCorrect = (runnerData.cheatType == CheatType.SpeedBoost && runnerData.CheatTimePhase == sol.racePhaseSolution);
        }
    }
}

