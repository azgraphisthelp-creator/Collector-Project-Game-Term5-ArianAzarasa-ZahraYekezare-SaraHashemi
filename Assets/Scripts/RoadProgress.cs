using UnityEngine;

public class RoadProgress : MonoBehaviour
{
    public Transform progressStart;
    public Transform progressEnd;


    public float GetProgress(Vector3 playerPosition)
    {
        float progress = Mathf.InverseLerp(
            progressStart.position.z,
            progressEnd.position.z,
            playerPosition.z
        );

        return Mathf.Clamp01(progress);
    }
}