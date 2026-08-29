using UnityEngine;
using SmallHedge.SoundManager;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("References")]
    public RaceManager raceManager;

    [Header("Transition")]
    public GameObject transitionObject; // e.g. a fade panel / loading screen with its own Animator or DOTween sequence
    public float transitionInDuration = 0.5f;
    public float transitionOutDuration = 0.5f;

    public List<GameObject> Journals; // List of GameObjects representing each level, to be activated/deactivated as needed

    public GameObject NextButton;
    public GameObject LevelButton;

    public GameObject TvScreen;
    private bool isTransitioning = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        if (raceManager != null)
            raceManager.OnRaceEnded += HandleRaceEnded;
    }

    void OnDisable()
    {
        if (raceManager != null)
            raceManager.OnRaceEnded -= HandleRaceEnded;
    }

    void HandleRaceEnded()
    {
        Debug.Log("GameManager notified: race ended.");
        NextButton.SetActive(true); // Show the Next

    }

    public void LoadJournal()
    {

    }

    public void GoToNextLevel()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToLevel(raceManager.currentLevelIndex + 1));
        LevelButton.SetActive(false); // Hide the Level button when transitioning to the next level
    }

    public void GoToLevel(int levelIndex)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToLevel(levelIndex));
        LevelButton.SetActive(false); // Hide the Level button when transitioning to the next level
    }

    public void LoadJournal(int journalIndex)
    {

        if (isTransitioning) return;
        SoundManager.StopMusic(); // Stop the music when transitioning to the journal
        StartCoroutine(TransitionToJournal(journalIndex));
        NextButton.SetActive(false); // Hide the Next button when transitioning to the journal
        // Deactivate all journals first

    }

    IEnumerator TransitionToJournal(int journalIndex)
    {
        isTransitioning = true;

        foreach (GameObject journal in Journals)
        {
            journal.SetActive(false);
        }

        if (transitionObject != null)
        {
            transitionObject.GetComponent<Animator>()?.SetTrigger("TransitionIn");
            yield return new WaitForSeconds(transitionInDuration);
        }

        TvScreen.SetActive(false); // Hide the TV screen when transitioning to the journal



        if (transitionObject != null)
        {
            yield return new WaitForSeconds(transitionOutDuration);
            transitionObject.GetComponent<Animator>()?.SetTrigger("TransitionOut");
            yield return new WaitForSeconds(transitionOutDuration + 0.5f);
        }
        // Activate the selected journal
        if (journalIndex >= 0 && journalIndex < Journals.Count)
        {
            Journals[journalIndex].SetActive(true);
        }

        yield return new WaitForSeconds(1f); // Optional: wait a bit before transitioning out
        LevelButton.SetActive(true); // Show the Next button after the journal is displayed

        isTransitioning = false;
    }

    IEnumerator TransitionToLevel(int levelIndex)
    {
        isTransitioning = true;

        if (transitionObject != null)
        {
            transitionObject.GetComponent<Animator>()?.SetTrigger("TransitionIn");
            yield return new WaitForSeconds(transitionInDuration);
        }



        if (transitionObject != null)
        {
            yield return new WaitForSeconds(transitionOutDuration); // Optional: wait a bit before transitioning out
            transitionObject.GetComponent<Animator>()?.SetTrigger("TransitionOut");


        }
        TvScreen.SetActive(true); // Hide the TV screen when transitioning to the level
        Journals.ForEach(journal => journal.SetActive(false)); // Deactivate all journals when transitioning to the level
        raceManager.LoadLevel(levelIndex);
        isTransitioning = false;
    }

}