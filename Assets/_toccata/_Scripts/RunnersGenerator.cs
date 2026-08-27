using NUnit.Framework;
using System;
using System.Collections.Generic;
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

    public Transform listOfParticipants;
    public GameObject listOfParticipantsItemPrefab;

    public List<LevelSettings> levelSettings;
    public int currentLevel = 0;


    public List<RunnerData> currentRunners = new List<RunnerData>();

    private int currentRunnerIndex = 0;


    private void Start()
    {
        UpdateGuidelinesUi();
    }

    [ContextMenu("generate new list of ppl")]
    public void GenerateListOfParticipants()
    {
        if (currentRunners.Count != 0) { 
            currentRunners.Clear();
        }
        for (int i = listOfParticipants.childCount - 1; i >= 0; i--)
        {
            Destroy(listOfParticipants.GetChild(i).gameObject);
        }

        RunnerData runnerData = GeneratePerson(Cheatos.IfnoMissmatch);
        currentRunners.Add(runnerData);
        GameObject listItem = Instantiate(listOfParticipantsItemPrefab, listOfParticipants);
        TextMeshProUGUI textMesh = listItem.GetComponent<TextMeshProUGUI>();
        textMesh.text = runnerData.runnerName;

        for (int i = 0; i < 5; i++) {
            runnerData = GeneratePerson(Cheatos.None);
            currentRunners.Add(runnerData);
            listItem = Instantiate(listOfParticipantsItemPrefab,listOfParticipants);
            textMesh = listItem.GetComponent<TextMeshProUGUI>();
            textMesh.text = runnerData.runnerName;

        }

        

    }

    public void PreviewNextRunner()
    {
        currentRunnerIndex++;
        currentRunnerIndex = Math.Clamp(currentRunnerIndex, 0, currentRunners.Count -1);
        personVisuals.DisplayPerson(currentRunners[currentRunnerIndex]);
    }
    public void PreviewPreviousRunner()
    {
        currentRunnerIndex--;
        currentRunnerIndex = Math.Clamp(currentRunnerIndex, 0, currentRunners.Count -1);
        personVisuals.DisplayPerson(currentRunners[currentRunnerIndex]);
    }




    [ContextMenu("generate person")]
    public RunnerData GeneratePerson(Cheatos cheatos)
    {
        // Create local object first to avoid recursion overwriting the class field
        RunnerData newPerson = new RunnerData
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
            cheatType = cheatos,
            runnerID = UnityEngine.Random.Range(500, 9999)
        };

        if (cheatos == Cheatos.IfnoMissmatch)
        {
            // Recursively generate fake data without touching newPerson until it's done
            newPerson.fakePersona = GeneratePerson(Cheatos.None);
        }

        currentPerson = newPerson;
        personVisuals.DisplayPerson(currentPerson);
        return currentPerson;
    }

    [ContextMenu("approve runner")]
    public void ApproveRunner()
    {
        bool isAllowed = IsCurrentRunnerValid();

        if (isAllowed)
        {
            // Add win logic / score points / grant entry
            print("Runner correctly approved!");

        }
        else
        {
            // Player made a mistake approving an illegal runner
            print("Mistake! You approved a banned runner.");
            PenalizePlayer();

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

        }
        else
        {
            // Player made a mistake rejecting a valid runner
            print("Mistake! You rejected a valid runner.");
            PenalizePlayer();

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


    [ContextMenu("Load Level 0")]
    public void LoadFirstLevel()
    {
        LoadLevel(0);
    }

    public void LoadLevel(int levelIndex)
    {
        // 1. Guard check for valid level index
        if (levelSettings == null || levelIndex < 0 || levelIndex >= levelSettings.Count)
        {
            Debug.LogWarning($"Level index {levelIndex} is out of bounds or levelSettings list is empty.");
            return;
        }

        // 2. Set current level index and retrieve settings
        currentLevel = levelIndex;
        LevelSettings settings = levelSettings[currentLevel];

        // 3. Update the guidelines for this level
        if (settings.guideLines != null)
        {
            currentGuidelines = settings.guideLines;
            UpdateGuidelinesUi();
        }

        // 4. Clear current UI elements and runners list
        currentRunners.Clear();
        currentRunnerIndex = 0;

        for (int i = listOfParticipants.childCount - 1; i >= 0; i--)
        {
            Destroy(listOfParticipants.GetChild(i).gameObject);
        }

        // 5. Generate runners defined by the level settings configuration
        foreach (cheatAmount cheatConfig in settings.runnerSettings)
        {
            for (int i = 0; i < cheatConfig.amount; i++)
            {
                RunnerData newRunner = GeneratePerson(cheatConfig.cheatType);
                currentRunners.Add(newRunner);

                // Populate UI item
                GameObject listItem = Instantiate(listOfParticipantsItemPrefab, listOfParticipants);
                TextMeshProUGUI textMesh = listItem.GetComponent<TextMeshProUGUI>();
                if (textMesh != null)
                {
                    textMesh.text = newRunner.runnerName;
                }
            }
        }

        // 6. Display the initial runner if any were generated
        if (currentRunners.Count > 0)
        {
            currentPerson = currentRunners[0];
            personVisuals.DisplayPerson(currentPerson);
        }
    }
}
