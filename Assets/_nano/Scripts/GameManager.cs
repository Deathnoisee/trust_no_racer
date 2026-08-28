using UnityEngine;
using System;
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
        // this is where you'd eventually branch into your Analyse Phase instead of
        // going straight to the next race — for now, just chain to the next level
        Debug.Log("GameManager notified: race ended.");
    }

    public void LoadJournal()
    {

    }

    public void GoToNextLevel()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToLevel(raceManager.currentLevelIndex + 1));
    }

    public void GoToLevel(int levelIndex)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToLevel(levelIndex));
    }

    public void LoadJournal(int journalIndex)
    {

        if (isTransitioning) return;
        StartCoroutine(TransitionToJournal(journalIndex));
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


        // Activate the selected journal
        if (journalIndex >= 0 && journalIndex < Journals.Count)
        {
            Journals[journalIndex].SetActive(true);
        }

        if (transitionObject != null)
        {
            yield return new WaitForSeconds(transitionOutDuration);
            transitionObject.GetComponent<Animator>()?.SetTrigger("TransitionOut");
        }

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

        raceManager.LoadLevel(levelIndex);

        if (transitionObject != null)
        {
            yield return new WaitForSeconds(transitionOutDuration);
            transitionObject.GetComponent<Animator>()?.SetTrigger("TransitionOut");
        }

        isTransitioning = false;
    }

}