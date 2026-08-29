using UnityEngine;
using TMPro;

public class NotepadManager : MonoBehaviour
{
    public TMP_InputField NotepadInputField;
    public bool AnalysisPhase { get; private set; } = false;
    public bool IsOpen { get; private set; } = true;
    public RaceManager raceManager;

    private void OnEnable()
    {
        raceManager.OnRaceEnded += SwitchAnalysisPhase;
    }
    public void SwitchAnalysisPhase()
    {
        AnalysisPhase = true;
        NotepadInputField.interactable = false;
        NotepadInputField.readOnly = true;
    }

    public void SetOpenState(bool open)
    {
        IsOpen = open;

        if (AnalysisPhase)
        {
            NotepadInputField.interactable = false;
            NotepadInputField.readOnly = true;
            return;
        }

        NotepadInputField.interactable = open;
        NotepadInputField.readOnly = !open;
        if (open)
        {
            NotepadInputField.ActivateInputField();
            NotepadInputField.readOnly = false;
            NotepadInputField.Select();
        }
        else
        {
            NotepadInputField.DeactivateInputField();
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            NotepadInputField.readOnly = true;
        }
    }

    public string GetNotepadText()
    {
        return NotepadInputField.text;
    }

    public void Init()
    {
        AnalysisPhase = false;
        NotepadInputField.interactable = true;
        NotepadInputField.text = "";
        IsOpen = true;
    }
}