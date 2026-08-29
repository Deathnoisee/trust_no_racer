using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class blood : MonoBehaviour
{
    [SerializeField] private Sprite GreenSprite;
    private Image image;
    private Sprite originalSprite;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalSprite = image.sprite;
    }
    public void ReceiveSolution(solution droppedSolution)
    {
        if (droppedSolution.isSolutionCorrect)
        {
            image.sprite = GreenSprite;
            RunnersGenerator.instance.currentRunners[RunnersGenerator.instance.currentRunnerIndex].drugTestCorrect = true;
        }
        else
        {
            Debug.Log("Incorrect solution. Try again.");
        }
        droppedSolution.gameObject.SetActive(false);
    }
    public void ResetBlood()
    {
        image.sprite = originalSprite;
    }
}
