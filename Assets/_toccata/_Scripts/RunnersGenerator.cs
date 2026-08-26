using UnityEngine;

public class RunnersGenerator : MonoBehaviour
{
    public SpriteView[] possibleHairs;
    public SpriteView[] possibleFaces;
    public SpriteView[] possibleShoes;
    public Color[] possibleHairColors;
    public Color[] possibleSkinColors;
    public Color[] possibleShirtColors;

    public string[] possibleNames;
    public Nationality[] possibleNationalities;

    RunnerData currentPerson;

    public RunnerFrontVisualisation personVisuals;

    [ContextMenu("generate person")]
    public void GeneratePerson()
    {
        currentPerson = new RunnerData
        {
            runnerName = possibleNames[Random.Range(0, possibleNames.Length)],
            runnerNationality = possibleNationalities[Random.Range(0, possibleNationalities.Length)],
            hair = possibleHairs[Random.Range(0, possibleHairs.Length)] ,
            shoes = possibleShoes[Random.Range(0, possibleShoes.Length)],
            hairColor = possibleHairColors[Random.Range(0, possibleHairColors.Length)],
            skinColor = possibleSkinColors[Random.Range(0, possibleSkinColors.Length)],
            shirtColor = possibleShirtColors[Random.Range(0, possibleShirtColors.Length)],
            shoesColor = possibleShirtColors[Random.Range(0, possibleShirtColors.Length)],



            runnerID = Random.Range(500, 9999) // Generates both valid and invalid IDs
        };

        personVisuals.DisplayPerson(currentPerson);
    }

}
