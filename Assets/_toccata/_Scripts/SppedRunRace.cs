using UnityEngine;

public class SppedRunRace : MonoBehaviour
{
    
    public bool speedRunRace = false;
    public float gameSpeed = 10;

    private void OnEnable()
    {
        GameManager.instance.raceManager.OnRaceEnded += RestoreNormalSpeed;
    }
    private void OnDisable()
    {
        GameManager.instance.raceManager.OnRaceEnded -= RestoreNormalSpeed;
    }


    private void Start()
    {
       
        if (speedRunRace)
        {
            Time.timeScale = gameSpeed;
        }
    }

    public void RestoreNormalSpeed()
    {
        Time.timeScale = 1;
    }


}
