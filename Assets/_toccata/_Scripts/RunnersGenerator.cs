using System;
using TMPro;
using UnityEngine;

public class RunnersGenerator : MonoBehaviour
{
    public SpriteView[] possibleHairs;
    public SpriteView[] possibleFaces;
    public SpriteView[] possibleShoes;
    public Color[] possibleHairColors;
    public Color[] possibleSkinColors;
    public Color[] possibleShirtColors;

    public int minAge = 18;
    public int maxAge = 40;


    public string[] possibleNames;
    public Nationality[] possibleNationalities;
    public Gender[] possibleGender;

    RunnerData currentPerson;

    public RunnerFrontVisualisation personVisuals;

    public LevelGuidelines currentGuidelines;
    public TextMeshProUGUI currentGuidelinesText;

    private void Start()
    {
        UpdateGuidelinesUi();
    }


    [ContextMenu("generate person")]
    public void GeneratePerson()
    {
        currentPerson = new RunnerData
        {
            runnerName = possibleNames[UnityEngine.Random.Range(0, possibleNames.Length)],
            runnerNationality = possibleNationalities[UnityEngine.Random.Range(0, possibleNationalities.Length)],
            gender = possibleGender[UnityEngine.Random.Range(0, possibleGender.Length)],
            age = UnityEngine.Random.Range(minAge, maxAge),

            hair = possibleHairs[UnityEngine.Random.Range(0, possibleHairs.Length)],
            shoes = possibleShoes[UnityEngine.Random.Range(0, possibleShoes.Length)],
            hairColor = possibleHairColors[UnityEngine.Random.Range(0, possibleHairColors.Length)],
            skinColor = possibleSkinColors[UnityEngine.Random.Range(0, possibleSkinColors.Length)],
            shirtColor = possibleShirtColors[UnityEngine.Random.Range(0, possibleShirtColors.Length)],
            shoesColor = possibleShirtColors[UnityEngine.Random.Range(0, possibleShirtColors.Length)],

            runnerID = UnityEngine.Random.Range(500, 9999) // Generates both valid and invalid IDs
        };

        personVisuals.DisplayPerson(currentPerson);
    }

    [ContextMenu("approve runner")]
    public void ApproveRunner()
    {
        bool isAllowed = IsCurrentRunnerValid();

        if (isAllowed)
        {
            // Add win logic / score points / grant entry
            print("Runner correctly approved!");
            AdvanceToNextRunner();
        }
        else
        {
            // Player made a mistake approving an illegal runner
            print("Mistake! You approved a banned runner.");
            PenalizePlayer();
            AdvanceToNextRunner();
        }
    }
    [ContextMenu("reject runner")]
    public void RejectRunner()
    {
        bool isAllowed = IsCurrentRunnerValid();

        if (!isAllowed)
        {
            // Add win logic / score points / rejected properly
            print("Runner correctly rejected!");
            AdvanceToNextRunner();
        }
        else
        {
            // Player made a mistake rejecting a valid runner
            print("Mistake! You rejected a valid runner.");
            PenalizePlayer();
            AdvanceToNextRunner();
        }
    }

    // Helper method to keep your code clean and prevent duplicate logic
    private bool IsCurrentRunnerValid()
    {
        if (currentGuidelines != null)
        {
            if (Array.IndexOf(currentGuidelines.bannedNationalities, currentPerson.runnerNationality) != -1)
            {
                print("Nationality is banned!");
                return false;
            }

            if (Array.IndexOf(currentGuidelines.bannedShoeSprites, currentPerson.shoes.frontView) != -1)
            {
                print("Shoes are banned!");
                return false;
            }
        }

        return true; // Passed all checks
    }

    private void PenalizePlayer()
    {
        print("YOU GOT IT WROOOOOOOOOOOONG");
    }

    private void AdvanceToNextRunner()
    {
        GeneratePerson();
    }

    [ContextMenu("Update guidelines")]
    public void UpdateGuidelinesUi()
    {
        if (currentGuidelines != null)
        {
            currentGuidelinesText.text = "";
            for (int i = 0; i < currentGuidelines.bannedStuff.Length; i++) {
                currentGuidelinesText.text += "-" + currentGuidelines.bannedStuff[i] + "\n";


            }

        }

    }
}
