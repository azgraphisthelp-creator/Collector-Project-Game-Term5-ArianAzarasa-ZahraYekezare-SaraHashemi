using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Forward Movement")]
    [SerializeField] private float forwardSpeed = 8f;

    [Header("Horizontal Movement")]
    [SerializeField] private float horizontalSpeed = 10f;
    [SerializeField] private float horizontalLimit = 3f;


    [Header("Start Game")]
    [SerializeField] private GameObject swipeUI;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private float fingerOffsetX;
    private bool isDragging;

    private bool canMove = true;
    private bool gameStarted = false;

    private void Start()
    {
        gameStarted = false;

        if (swipeUI != null)
            swipeUI.SetActive(true);

        if (animator != null)
            animator.SetBool("IsRunning", false);
    }

    private void Update()
    {
        if (!canMove)
            return;

        if (UIManager.Instance != null && !UIManager.Instance.CanInput())
            return;

        HandleInput();

        if (!gameStarted)
            return;

        MoveForward();
    }

    private void MoveForward()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
    }

    private void HandleInput()
    {
#if UNITY_EDITOR
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    private void HandleMouse()
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

            if (!gameStarted && Mathf.Abs(delta) > 0.005f)
            {
                StartGame();
            }

            MoveHorizontal(delta);

            fingerOffsetX = Input.mousePosition.x;
        }
    }

    private void HandleTouch()
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

            if (!gameStarted && Mathf.Abs(delta) > 0.005f)
            {
                StartGame();
            }

            MoveHorizontal(delta);

            fingerOffsetX = touch.position.x;
        }
    }

    private void StartGame()
    {
        gameStarted = true;

        if (swipeUI != null)
            swipeUI.SetActive(false);

        if (animator != null)
            animator.SetBool("IsRunning", true);
    }

    private void MoveHorizontal(float delta)
    {
        Vector3 pos = transform.position;

        pos.x += delta * horizontalSpeed;
        pos.x = Mathf.Clamp(pos.x, -horizontalLimit, horizontalLimit);

        transform.position = pos;
    }

    public void StopMovement()
    {
        canMove = false;

        if (animator != null)
            animator.SetBool("IsRunning", false);
    }

    public void ResumeMovement()
    {
        canMove = true;

        if (gameStarted && animator != null)
            animator.SetBool("IsRunning", true);
    }

        public float GetForwardSpeed()
{
    return forwardSpeed;
}
}