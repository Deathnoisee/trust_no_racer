using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunnerFrontVisualisation : MonoBehaviour
{
    [Header("Full Body View")]
    public Image hairRenderer;
    public Image shoeRenderer;
    public Image bodyRenderer;
    public Image headRenderer;
    public Image faceRenderer;
    public Image armsRenderer;
    public Image armsShirtRenderer;

    [Header("ID Card View")]
    public Image idCardHair;
    public Image idCardBody;
    public Image idCardHead;
    public Image idCardFace;
    public Image idCardArms;
    public Image idCardArmsShirt;
    public Image idCardShoes;

    [Header("Text Info")]
    public TextMeshProUGUI runnerId;
    public TextMeshProUGUI runnerName;
    public TextMeshProUGUI runnerAge;
    public TextMeshProUGUI runnerGender;
    public TextMeshProUGUI runnerNationality;

    public void DisplayPerson(RunnerData data)
    {
        if (data == null) return;

        



        // ---------------------------------------------------------
        // 2. ID CARD VIEW
        // ---------------------------------------------------------
        // Apply Sprites

        if (idCardHair != null && data.hair != null) idCardHair.sprite = data.hair.frontView;
        if (idCardShoes != null && data.shoes != null) idCardShoes.sprite = data.shoes.frontView;

        // Apply Hair & Shoe Colors
        if (idCardHair != null) idCardHair.color = data.hairColor;
        if (idCardShoes != null) idCardShoes.color = data.shoesColor;

        // Apply Shirt & Skin Colors
        if (idCardBody != null) idCardBody.color = data.shirtColor;
        if (idCardArmsShirt != null) idCardArmsShirt.color = data.shirtColor;

        if (idCardHead != null) idCardHead.color = data.skinColor;
        if (idCardFace != null) idCardFace.color = data.skinColor;
        if (idCardArms != null) idCardArms.color = data.skinColor;

        // ---------------------------------------------------------
        // 3. TEXT INFO
        // ---------------------------------------------------------
        if (runnerId != null) runnerId.text = data.runnerID.ToString();
        if (runnerName != null) runnerName.text = data.runnerName;
        if (runnerAge != null) runnerAge.text = data.age.ToString();
        if (runnerNationality != null) runnerNationality.text = data.runnerNationality.ToString();
        if (runnerGender != null) runnerGender.text = data.gender.ToString();

        if(data.cheatType == Cheatos.IfnoMissmatch)
        {
            data = data.fakePersona;
        }
        // ---------------------------------------------------------
        // 1. FULL BODY VIEW
        // ---------------------------------------------------------
        // Apply Sprites
        if (hairRenderer != null && data.hair != null) hairRenderer.sprite = data.hair.frontView;
        if (shoeRenderer != null && data.shoes != null) shoeRenderer.sprite = data.shoes.frontView;

        // Apply Hair & Shoe Colors
        if (hairRenderer != null) hairRenderer.color = data.hairColor;
        if (shoeRenderer != null) shoeRenderer.color = data.shoesColor;

        // Apply Shirt & Skin Colors
        if (bodyRenderer != null) bodyRenderer.color = data.shirtColor;
        if (armsShirtRenderer != null) armsShirtRenderer.color = data.shirtColor;

        if (headRenderer != null) headRenderer.color = data.skinColor;
        if (faceRenderer != null) faceRenderer.color = data.skinColor;
        if (armsRenderer != null) armsRenderer.color = data.skinColor;

    }
}