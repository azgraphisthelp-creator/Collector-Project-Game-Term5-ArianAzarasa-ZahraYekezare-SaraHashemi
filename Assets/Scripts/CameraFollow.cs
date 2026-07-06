using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    private Vector3 offset;

    private bool canFollow = true;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (!canFollow)
            return;

        transform.position = target.position + offset;
    }

    public void StopFollow()
    {
        canFollow = false;
    }

    public void StartFollow()
    {
        canFollow = true;
    }
}