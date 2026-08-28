using UnityEngine;
using TMPro;

public class ListImporter : MonoBehaviour
{
    public GameObject listOfParticipantsItemPrefab;


    [ContextMenu("Load List of Names")]
    public void loadListNames()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (RunnerData newRunner in RunnersGenerator.instance.currentRunners)
        {
            GameObject listItem = Instantiate(listOfParticipantsItemPrefab, transform);
            TextMeshProUGUI textMesh = listItem.GetComponent<TextMeshProUGUI>();
            if (textMesh != null)
            {
                textMesh.text = "";
                textMesh.text = newRunner.runnerName;
                textMesh.text += " " + newRunner.runnerLastName;
            }
        }
    }

}
