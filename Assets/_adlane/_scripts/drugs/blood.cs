using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class blood : MonoBehaviour
{
    private Image image;
    public void ReceiveSolution(solution droppedSolution)
    {
        if (droppedSolution.isSolutionCorrect)
        {
            image = GetComponent<Image>();

            image.DOColor(Color.green, 0.5f).OnComplete(() =>
            {
                Debug.Log("Correct solution!");
            });
        }
        else
        {
            Debug.Log("Incorrect solution. Try again.");
        }
        droppedSolution.gameObject.SetActive(false);
    }
    public void ResetBlood()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }
        image.color = Color.red;
    }
}
