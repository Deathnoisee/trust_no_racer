using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestOptionButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleTmp;
    [SerializeField] TextMeshProUGUI amountTmp;
    [SerializeField] GameObject lockObj;
    [SerializeField] Image holderImage;
    

    public Color lockColor =new Color(56,56,56);

    public void SetAsLocked()
    {
        holderImage.color = lockColor;
        lockObj.SetActive(true);
        amountTmp.gameObject.SetActive(false);
        titleTmp.gameObject.SetActive(false);
        
        Button btn = GetComponent<Button>();
        btn.enabled = false;

    }


    public void SetAsOpen()
    {
        holderImage.color = Color.white;
        lockObj.SetActive(false);
        amountTmp.gameObject.SetActive(true);
        titleTmp.gameObject.SetActive(true);
        Button btn = GetComponent<Button>();
        btn.enabled = true;
    }

    public void SetAmount(int current,int max)
    {
        amountTmp.text = current.ToString() + "/" + max.ToString();
    }




}
