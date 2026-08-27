using UnityEngine;
using TMPro;

public class NotepadManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField NotepadInputField;
    public bool AnalysisPhase { get; private set; } = false;
    public bool IsOpen { get; private set; } = true;

    public void SwitchAnalysisPhase()
    {
        AnalysisPhase = true;
        NotepadInputField.interactable = false;
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

        if (open)
        {
            NotepadInputField.ActivateInputField();
            NotepadInputField.readOnly = false;
            NotepadInputField.Select();
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