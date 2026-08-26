using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunnerFrontVisualisation : MonoBehaviour
{
    public Image hairRenderer;
    public Image shoeRenderer;
    public Image bodyRenderer;
    public Image headRenderer;
    public Image faceRenderer;
    public Image armsRenderer;
    public Image armsShirtRenderer;

    public TextMeshProUGUI runnerId;

    public void DisplayPerson(RunnerData data)
    {
        if (data == null) return;

        // Apply Front Sprites (guard against null SpriteViews)
        if (data.hair != null) hairRenderer.sprite = data.hair.frontView;
        if (data.shoes != null) shoeRenderer.sprite = data.shoes.frontView;

        // Apply Colors
        hairRenderer.color = data.hairColor;
        shoeRenderer.color = data.shoesColor;

        // Body, head, face, and arms share skin and shirt colors
        bodyRenderer.color = data.shirtColor;
        armsShirtRenderer.color = data.shirtColor;

        headRenderer.color = data.skinColor;
        faceRenderer.color = data.skinColor;
        armsRenderer.color = data.skinColor;

        // Apply Text Info
        if (runnerId != null)
        {
            runnerId.text = data.runnerID.ToString();
        }
    }
}
