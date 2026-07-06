using UnityEngine;

public class MagnetCollector : MonoBehaviour
{
    [SerializeField] private Transform magnrtPoint;
    [SerializeField] private float magnetForce = 20f;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Ball"))
            return;

        Rigidbody rb = other.GetComponentInParent<Rigidbody>();

        if (rb == null)
            return;

        Vector3 direction =
            (magnrtPoint.position - rb.transform.position).normalized;

        rb.AddForce(
        direction * magnetForce,
        ForceMode.Acceleration);
    }
}