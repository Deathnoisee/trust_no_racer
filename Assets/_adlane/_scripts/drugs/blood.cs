using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class blood : MonoBehaviour
{
    [SerializeField] public Sprite GreenSprite;
    private Image image;
    public Sprite originalSprite;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.sprite = originalSprite;
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

}
