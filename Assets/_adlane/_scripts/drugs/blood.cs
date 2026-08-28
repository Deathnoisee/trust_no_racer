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
            Destroy(droppedSolution.gameObject);
            Debug.Log("Incorrect solution. Try again.");
        }

        Destroy(droppedSolution.gameObject);
    }
}
