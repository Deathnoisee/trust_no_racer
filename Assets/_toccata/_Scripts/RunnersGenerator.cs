using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// TODO: Add score and win condition / lose condition
// - click on confirm cheaters
// - count the cheaters selected and compare with the actual cheaters in the level
// - show a win or lose screen based on the result before load journal scene
// - then restart level or load journal scene

public class RunnersGenerator : MonoBehaviour
{
    public static RunnersGenerator instance;

    public SpriteView[] possibleHairs;
    public SpriteView[] possibleFaces;
    public SpriteView[] possibleShoes;
    public Color[] possibleHairColors;
    public Color[] possibleSkinColors;
    public Color[] possibleShirtColors;

    public int minAge = 18;
    public int maxAge = 40;

    public string[] possibleNames;
    public string[] possibleLastNames;
    public Nationality[] possibleNationalities;
    public Gender[] possibleGender;

    RunnerData currentPerson;

    public RunnerFrontVisualisation personVisuals;

    public LevelGuidelines currentGuidelines;
    public TextMeshProUGUI currentGuidelinesText;

    // public Transform listOfParticipants;
    // public GameObject listOfParticipantsItemPrefab;

    public List<LevelSettings> levelSettings;
    public int currentLevelIndex = 0;

    public List<RunnerData> currentRunners = new List<RunnerData>();

    public int currentRunnerIndex = 0;
    public event Action<RunnerData> analysePlayer;
    // public event Action<RunnerData> trajectoryButton;
    public event Action<RunnerData> tubesButton;
    public event Action<RaceDataType, RunnerData> analyseTrajectory;

    // Track unused shirt colors for the current generation batch
    private List<Color> availableShirtColors = new List<Color>();

    // Track unused names and last names for the current generation batch
    private List<string> availableNames = new List<string>();
    private List<string> availableLastNames = new List<string>();

    [Header("Cheating Detection")]

    [SerializeField] GameObject cheatLebel;

    [SerializeField] public TestOptionButton drugBtn;
    [SerializeField] public TestOptionButton trajectoryBtn;
    [SerializeField] public TestOptionButton varCheckBtn;
    [SerializeField] public TestOptionButton lieDetectorBtn;

    public int drugTestsLeft = 0;
    public int trajectoryTestsLeft = 0;
    public int varCheckLeft = 0;
    public int lieDetectorLeft = 0;

    public LevelSettings currentLevel;

    [Header("End of Level")]
    public GameObject ShowResultsScreen;
    private int selectedCheatersCount = 0;
    private int totalCheatersCount = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UpdateGuidelinesUi();
        currentLevel = levelSettings[currentLevelIndex];
    }

    [SerializeField] GameObject trajectoryObj;
    [SerializeField] GameObject tubesObj;
    public void ShowTrajectory()
    {
        //hide anything that was shown
        tubesObj.SetActive(false);
        analyseTrajectory?.Invoke(RaceDataType.Trajectory, currentRunners[currentRunnerIndex]);
        //show trajectory
        trajectoryObj.SetActive(true);

    }

    public void ShowTubes()
    {
        //hide anything that was shown
        trajectoryObj.SetActive(false);
        tubesButton?.Invoke(currentRunners[currentRunnerIndex]);
        //show tubes

        tubesObj.SetActive(true);

    }

    public void HideTrajectoryAndTubes()
    {

        trajectoryObj.SetActive(false);
        tubesObj.SetActive(false);
    }






    /// <summary>
    /// Resets and fills the pool of available shirt colors.
    /// </summary>
    private void ResetShirtColorPool()
    {
        availableShirtColors = new List<Color>(possibleShirtColors);
    }

    /// <summary>
    /// Resets and fills the pool of available first names.
    /// </summary>
    private void ResetNamePool()
    {
        availableNames = new List<string>(possibleNames);
    }

    /// <summary>
    /// Resets and fills the pool of available last names.
    /// </summary>
    private void ResetLastNamePool()
    {
        availableLastNames = new List<string>(possibleLastNames);
    }

    /// <summary>
    /// Pulls a unique shirt color from the pool. Falls back to a random color if exhausted.
    /// </summary>
    private Color GetUniqueShirtColor()
    {
        if (availableShirtColors == null || availableShirtColors.Count == 0)
        {
            Debug.LogWarning("Run out of unique shirt colors! Resetting pool or picking random.");
            ResetShirtColorPool();

            // If possibleShirtColors is empty in Inspector, default to white
            if (availableShirtColors.Count == 0) return Color.white;
        }

        int index = UnityEngine.Random.Range(0, availableShirtColors.Count);
        Color selectedColor = availableShirtColors[index];
        availableShirtColors.RemoveAt(index);
        return selectedColor;
    }

    /// <summary>
    /// Pulls a unique first name from the pool. Falls back to a random name if exhausted.
    /// </summary>
    private string GetUniqueName()
    {
        if (availableNames == null || availableNames.Count == 0)
        {
            Debug.LogWarning("Run out of unique names! Resetting pool or picking random.");
            ResetNamePool();

            if (availableNames.Count == 0) return "Unknown";
        }

        int index = UnityEngine.Random.Range(0, availableNames.Count);
        string selectedName = availableNames[index];
        availableNames.RemoveAt(index);
        return selectedName;
    }

    /// <summary>
    /// Pulls a unique last name from the pool. Falls back to a random last name if exhausted.
    /// </summary>
    private string GetUniqueLastName()
    {
        if (availableLastNames == null || availableLastNames.Count == 0)
        {
            Debug.LogWarning("Run out of unique last names! Resetting pool or picking random.");
            ResetLastNamePool();

            if (availableLastNames.Count == 0) return "Unknown";
        }

        int index = UnityEngine.Random.Range(0, availableLastNames.Count);
        string selectedLastName = availableLastNames[index];
        availableLastNames.RemoveAt(index);
        return selectedLastName;
    }

    [ContextMenu("generate new list of ppl")]
    public void GenerateListOfParticipants()
    {
        ResetShirtColorPool(); // Reset pool before bulk generation
        ResetNamePool();
        ResetLastNamePool();

        if (currentRunners.Count != 0)
        {
            currentRunners.Clear();
        }
        // for (int i = listOfParticipants.childCount - 1; i >= 0; i--)
        // {
        //     Destroy(listOfParticipants.GetChild(i).gameObject);
        // }

        RunnerData runnerData = GeneratePersonInternal(CheatType.InfoMismatch);

        currentRunners.Add(runnerData);
        // GameObject listItem = Instantiate(listOfParticipantsItemPrefab, listOfParticipants);
        // TextMeshProUGUI textMesh = listItem.GetComponent<TextMeshProUGUI>();
        // textMesh.text = runnerData.runnerName;

        for (int i = 0; i < 5; i++)
        {
            runnerData = GeneratePersonInternal(CheatType.None);
            currentRunners.Add(runnerData);
            // listItem = Instantiate(listOfParticipantsItemPrefab, listOfParticipants);
            // textMesh = listItem.GetComponent<TextMeshProUGUI>();
            // textMesh.text = runnerData.runnerName;
        }
    }

    public void InitializePhase()
    {
        PreviewCurrentRunner();
        cheatLebel.SetActive(currentRunners[currentRunnerIndex].selectedAsCheater);

        // 1. Assign remaining counts from current level settings
        drugTestsLeft = currentLevel.drugTests;
        trajectoryTestsLeft = currentLevel.trajectoryTests;
        varCheckLeft = currentLevel.varCheck;
        lieDetectorLeft = currentLevel.lieDetection;
        UpdateTestButtons(true);


    }

    public void UpdateTestButtons(bool lockAtZero)
    {
        // Helper method to keep code clean and eliminate repetitive logic
        void UpdateButton(TestOptionButton button, int testsLeft, int maxTests)
        {
            if (maxTests <= 0)
            {
                button.SetAsLocked();
                return;
            }

            // If lockAtZero is true and we've run out, lock it; otherwise keep it open to display 0/Max
            if (testsLeft <= 0 && lockAtZero)
            {
                button.SetAsLocked();
            }
            else
            {
                button.SetAsOpen();
                button.SetAmount(testsLeft, maxTests);
                if (testsLeft <= 0)
                {
                    if (button == drugBtn)
                    {
                        if (!currentRunners[currentRunnerIndex].earlyBloodTestDone && !currentRunners[currentRunnerIndex].midBloodTestDone && !currentRunners[currentRunnerIndex].lateBloodTestDone)
                        {
                            button.SetAsNoMoreUses();
                        }
                        else
                        {
                            button.AlreadyUsed();
                        }
                    }
                    else if (button == trajectoryBtn)
                    {
                        if (!currentRunners[currentRunnerIndex].trajectoryDone)
                        {
                            button.SetAsNoMoreUses();
                        }
                        else
                        {
                            button.AlreadyUsed();
                        }
                    }
                    else if (button == varCheckBtn)
                    {
                        if (!currentRunners[currentRunnerIndex].varCheckDone)
                        {
                            button.SetAsNoMoreUses();
                        }
                        else
                        {
                            button.AlreadyUsed();
                        }
                    }
                    else if (button == lieDetectorBtn)
                    {
                        if (!currentRunners[currentRunnerIndex].lieCheckDone)
                        {
                            button.SetAsNoMoreUses();
                        }
                        else
                        {
                            button.AlreadyUsed();
                        }
                    }
                }
            }
        }

        UpdateButton(drugBtn, drugTestsLeft, currentLevel.drugTests);
        UpdateButton(trajectoryBtn, trajectoryTestsLeft, currentLevel.trajectoryTests);
        UpdateButton(varCheckBtn, varCheckLeft, currentLevel.varCheck);
        UpdateButton(lieDetectorBtn, lieDetectorLeft, currentLevel.lieDetection);
    }



    public void PreviewCurrentRunner()
    {
        personVisuals.DisplayPerson(currentRunners[currentRunnerIndex]);

        cheatLebel.SetActive(currentRunners[currentRunnerIndex].selectedAsCheater);


        analysePlayer?.Invoke(currentRunners[currentRunnerIndex]);
    }


    public void PreviewNextRunner()
    {
        currentRunnerIndex++;
        currentRunnerIndex = Math.Clamp(currentRunnerIndex, 0, currentRunners.Count - 1);
        personVisuals.DisplayPerson(currentRunners[currentRunnerIndex]);
        cheatLebel.SetActive(currentRunners[currentRunnerIndex].selectedAsCheater);
        HideTrajectoryAndTubes();
        UpdateTestButtons(false);


        analysePlayer?.Invoke(currentRunners[currentRunnerIndex]);
    }

    public void PreviewPreviousRunner()
    {
        currentRunnerIndex--;
        currentRunnerIndex = Math.Clamp(currentRunnerIndex, 0, currentRunners.Count - 1);
        personVisuals.DisplayPerson(currentRunners[currentRunnerIndex]);
        cheatLebel.SetActive(currentRunners[currentRunnerIndex].selectedAsCheater);
        HideTrajectoryAndTubes();
        UpdateTestButtons(false);

        analysePlayer?.Invoke(currentRunners[currentRunnerIndex]);
    }

    public void SetCurrentRunnerAsCheater()
    {
        currentRunners[currentRunnerIndex].selectedAsCheater = true;
        cheatLebel.SetActive(currentRunners[currentRunnerIndex].selectedAsCheater);
        selectedCheatersCount++;
    }


    public void EmptyList()
    {
        if (currentRunners.Count != 0)
        {
            currentRunners.Clear();
        }
        // for (int i = listOfParticipants.childCount - 1; i >= 0; i--)
        // {
        //     Destroy(listOfParticipants.GetChild(i).gameObject);
        // }
        currentRunnerIndex = 0;
        ResetShirtColorPool();
        ResetNamePool();
        ResetLastNamePool();
    }

    [ContextMenu("generate person")]
    public RunnerData GeneratePersonContextMenu()
    {
        // When triggering context menu individually, ensure there's a color pool
        if (availableShirtColors.Count == 0)
        {
            ResetShirtColorPool();
        }

        if (availableNames.Count == 0)
        {
            ResetNamePool();
        }

        if (availableLastNames.Count == 0)
        {
            ResetLastNamePool();
        }

        RunnerData runner = GeneratePersonInternal(CheatType.None);
        currentRunners.Add(runner);

        // GameObject listItem = Instantiate(listOfParticipantsItemPrefab, listOfParticipants);
        // TextMeshProUGUI textMesh = listItem.GetComponent<TextMeshProUGUI>();
        // if (textMesh != null)
        // {
        //     textMesh.text = runner.runnerName;
        // }

        return runner;
    }

    public RunnerData GeneratePerson(CheatType cheatos)
    {
        return GeneratePersonInternal(cheatos);
    }

    private RunnerData GeneratePersonInternal(CheatType cheatos, bool addToList = true)
    {
        Color uniqueShirtColor = GetUniqueShirtColor();
        string uniqueName = GetUniqueName();
        string uniqueLastName = GetUniqueLastName();

        RunnerData newPerson = new RunnerData
        {
            runnerName = uniqueName,
            runnerLastName = uniqueLastName,
            runnerNationality = possibleNationalities[UnityEngine.Random.Range(0, possibleNationalities.Length)],
            gender = possibleGender[UnityEngine.Random.Range(0, possibleGender.Length)],
            age = UnityEngine.Random.Range(minAge, maxAge),

            hair = possibleHairs[UnityEngine.Random.Range(0, possibleHairs.Length)],
            shoes = possibleShoes[UnityEngine.Random.Range(0, possibleShoes.Length)],
            hairColor = possibleHairColors[UnityEngine.Random.Range(0, possibleHairColors.Length)],
            skinColor = possibleSkinColors[UnityEngine.Random.Range(0, possibleSkinColors.Length)],
            shirtColor = uniqueShirtColor,
            shoesColor = uniqueShirtColor,
            cheatType = cheatos,
            face = possibleFaces[UnityEngine.Random.Range(0, possibleFaces.Length)],
            runnerID = UnityEngine.Random.Range(500, 9999)
        };

        if (cheatos == CheatType.InfoMismatch)
        {
            newPerson.fakePersona = GeneratePersonInternal(CheatType.None, false);
        }

        currentPerson = newPerson;
        personVisuals.DisplayPerson(currentPerson);
        if (addToList) currentRunners.Add(currentPerson);

        return currentPerson;
    }

    [ContextMenu("approve runner")]
    public void ApproveRunner()
    {
        bool isAllowed = IsCurrentRunnerValid();

        if (isAllowed)
        {
            print("Runner correctly approved!");
        }
        else
        {
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
            print("Runner correctly rejected!");
        }
        else
        {
            print("Mistake! You rejected a valid runner.");
            PenalizePlayer();
        }
    }

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

        return true;
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
            for (int i = 0; i < currentGuidelines.bannedStuff.Length; i++)
            {
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
        if (levelSettings == null || levelIndex < 0 || levelIndex >= levelSettings.Count)
        {
            Debug.LogWarning($"Level index {levelIndex} is out of bounds or levelSettings list is empty.");
            return;
        }

        currentLevelIndex = levelIndex;
        LevelSettings settings = levelSettings[currentLevelIndex];

        if (settings.guideLines != null)
        {
            currentGuidelines = settings.guideLines;
            UpdateGuidelinesUi();
        }

        currentRunners.Clear();
        currentRunnerIndex = 0;

        // for (int i = listOfParticipants.childCount - 1; i >= 0; i--)
        // {
        //     Destroy(listOfParticipants.GetChild(i).gameObject);
        // }

        // Reset the shirt color, name, and last name pools once for the entire level generation pass
        ResetShirtColorPool();
        ResetNamePool();
        ResetLastNamePool();

        foreach (cheatAmount cheatConfig in settings.runnerSettings)
        {
            // for (int i = 0; i < cheatConfig.amount; i++)
            // {
            //     RunnerData newRunner = GeneratePersonInternal(cheatConfig.cheatType);
            //     currentRunners.Add(newRunner);

            //     GameObject listItem = Instantiate(listOfParticipantsItemPrefab, listOfParticipants);
            //     TextMeshProUGUI textMesh = listItem.GetComponent<TextMeshProUGUI>();
            //     if (textMesh != null)
            //     {
            //         textMesh.text = newRunner.runnerName;
            //     }
            // }
        }

        if (currentRunners.Count > 0)
        {
            currentPerson = currentRunners[0];
            personVisuals.DisplayPerson(currentPerson);
        }
    }

    public void ConfirmCheaters()
    {
        foreach (var runner in currentRunners)
        {
            totalCheatersCount += runner.cheatType != CheatType.None ? 1 : 0;
            if (runner.selectedAsCheater && runner.cheatType != CheatType.None)
            {
                selectedCheatersCount++;
            }
        }

        if (selectedCheatersCount == totalCheatersCount)
        {
            print("YOU WIN");
            ShowResultsScreen.SetActive(true);
        }
        else
        {
            print("YOU LOSE");
            ShowResultsScreen.SetActive(true);
        }
    }
    private void CountTotalScore()
    {
        float score = selectedCheatersCount/totalCheatersCount;
        if (score >= 0.99f)
        {
            print("3 stars");
            //TODO: feed the script 3 stars
            ShowResultsScreen.SetActive(true);
            ShowResultsScreen.GetComponent<StarsGenerator>().GenerateStars(3);

        }
        else if (score >= 0.66f)
        {
            print("2 stars");
            //TODO: feed the script 2 stars
            ShowResultsScreen.SetActive(true);
            ShowResultsScreen.GetComponent<StarsGenerator>().GenerateStars(2);

        }
        else if (score >= 0.33f)
        {
            print("1 star");
            
            ShowResultsScreen.SetActive(true);
            ShowResultsScreen.GetComponent<StarsGenerator>().GenerateStars(1);
        }
        else
        {
            print("YOU LOSE");
            ShowResultsScreen.SetActive(true);
        }
    }
}