using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Forward Movement")]
    [SerializeField] private float forwardSpeed = 8f;

    [Header("Horizontal Movement")]
    [SerializeField] private float horizontalSpeed = 10f;
    [SerializeField] private float horizontalLimit = 3f;

    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform finishPoint;

    private float fingerOffsetX;
    private bool isDragging;

    private bool canMove = true;

    void Update()
    {
        if (!canMove)
            return;

        // ⭐ IMPORTANT: UI PAUSE CHECK
        if (UIManager.Instance != null && !UIManager.Instance.CanInput())
            return;

        MoveForward();
        HandleInput();
        UpdateProgress();
    }

    void MoveForward()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
    }

    void HandleInput()
    {
#if UNITY_EDITOR
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            fingerOffsetX = Input.mousePosition.x;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            float delta = (Input.mousePosition.x - fingerOffsetX) / Screen.width;
            MoveHorizontal(delta);

            fingerOffsetX = Input.mousePosition.x;
        }
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            fingerOffsetX = touch.position.x;
        }

        if (touch.phase == TouchPhase.Moved)
        {
            float delta = (touch.position.x - fingerOffsetX) / Screen.width;
            MoveHorizontal(delta);

            fingerOffsetX = touch.position.x;
        }
    }

    void MoveHorizontal(float delta)
    {
        Vector3 pos = transform.position;

        pos.x += delta * horizontalSpeed;

        pos.x = Mathf.Clamp(pos.x, -horizontalLimit, horizontalLimit);

        transform.position = pos;
    }

    public void StopMovement()
    {
        canMove = false;
    }

    public void ResumeMovement()
    {
        canMove = true;
    }

    void UpdateProgress()
    {
        float distanceTravelled = transform.position.z - startPoint.position.z;
        float totalDistance = finishPoint.position.z - startPoint.position.z;

        float progress = Mathf.Clamp01(distanceTravelled / totalDistance);

        UIManager.Instance.UpdateProgress(progress);
    }
}