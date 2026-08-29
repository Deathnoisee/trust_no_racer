using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestOptionButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleTmp;
    [SerializeField] TextMeshProUGUI amountTmp;
    [SerializeField] GameObject lockObj;
    [SerializeField] Image holderImage;
    [SerializeField] GameObject noMoreUsesObj;
    [SerializeField] GameObject alreadyUsedObj;
    private RunnerData currentRunner;



    public Color lockColor = new Color(56, 56, 56);

    public void SetAsLocked()
    {
        holderImage.color = lockColor;
        lockObj.SetActive(true);
        amountTmp.gameObject.SetActive(false);
        titleTmp.gameObject.SetActive(false);
        noMoreUsesObj.SetActive(false);
        alreadyUsedObj.SetActive(false);

        Button btn = GetComponent<Button>();
        btn.enabled = false;
    }


    public void SetAsOpen()
    {
        holderImage.color = Color.white;
        lockObj.SetActive(false);
        amountTmp.gameObject.SetActive(true);
        titleTmp.gameObject.SetActive(true);
        noMoreUsesObj.SetActive(false);
        alreadyUsedObj.SetActive(false);
        Button btn = GetComponent<Button>();
        btn.enabled = true;
    }


    public void SetAmount(int current, int max)
    {
        currentRunner = RunnersGenerator.instance.currentRunners[RunnersGenerator.instance.currentRunnerIndex];
        if (currentRunner.selectedAsCheater)
        {
            return;
        }
        amountTmp.text = current.ToString() + "/" + max.ToString();


        if (current >= 0)
        {
            SetAsOpen();
        }

    }

    public void SetAsNoMoreUses()
    {
        amountTmp.gameObject.SetActive(false);
        titleTmp.gameObject.SetActive(false);
        lockObj.SetActive(false);
        alreadyUsedObj.SetActive(false);
        noMoreUsesObj.SetActive(true);
    }

    public void AlreadyUsed()
    {
        amountTmp.gameObject.SetActive(false);
        noMoreUsesObj.gameObject.SetActive(false);
        lockObj.SetActive(false);
        titleTmp.gameObject.SetActive(true);
        alreadyUsedObj.SetActive(true);
    }

}
