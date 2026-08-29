using UnityEngine;

public class SppedRunRace : MonoBehaviour
{

    public bool speedRunRace = false;
    public float gameSpeed = 10;
    private bool switchSpeed = false;




    private void Start()
    {
        if (speedRunRace)
        {
            Time.timeScale = gameSpeed;
        }
    }


    public void SetSpeedRunRace()
    {
        if (switchSpeed)
        {
            Time.timeScale = 1;
            switchSpeed = false;
        }
        else
        {
            Time.timeScale = gameSpeed;
            switchSpeed = true;
        }

    }


}
