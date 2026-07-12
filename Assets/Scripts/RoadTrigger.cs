using UnityEngine;

public class RoadTrigger : MonoBehaviour
{
    private bool triggered;

  private void OnTriggerEnter(Collider other)
{
    if (triggered)
        return;

    if (!other.CompareTag("Player"))
        return;

    triggered = true;

    Invoke(nameof(SpawnNext), 0.2f);
}


private void SpawnNext()
{
    RoadManager.Instance.PlayerPassedRoad();
}
}