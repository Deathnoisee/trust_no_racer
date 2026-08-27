using UnityEngine;
using UnityEngine.Splines;

public class TrackSetup : MonoBehaviour
{
    public string trackName;
    public SplineContainer spline;
    public Transform startPoint;
    public Transform[] spawnPoints;

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
