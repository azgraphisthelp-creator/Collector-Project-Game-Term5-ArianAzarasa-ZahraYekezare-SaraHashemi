using UnityEngine;

public class Ball : MonoBehaviour
{
    public bool IsCollected { get; set; }

    private Rigidbody rb;
    private Renderer rend;

    private int stackIndex;

    private bool isDepositing;
    private Vector3 depositTarget;

    private bool arrived;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponentInChildren<Renderer>();

        SetRandomColor();
    }

    private void SetRandomColor()
    {
        if (rend == null) return;

        rend.material.color = Random.ColorHSV();
    }

    public void SetStackIndex(int index)
    {
        stackIndex = index;
    }

    public void GoToDeposit(Vector3 target)
    {
        isDepositing = true;
        depositTarget = target;

        IsCollected = false;
        arrived = false;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
        }
    }

 private void FixedUpdate()
{
    if (rb == null) return;

    // ================= DEPOSIT =================
    if (isDepositing)
    {
        Vector3 dir = depositTarget - rb.position;
        float dist = dir.magnitude;

        // نزدیک مقصد سرعت کمتر شود
        float force = dist < 1.5f ? 10f : 20f;

        rb.AddForce(dir.normalized * force, ForceMode.Acceleration);

        // محدود کردن سرعت
        if (rb.velocity.magnitude > 10f)
            rb.velocity = rb.velocity.normalized * 10f;

        // وقتی نزدیک شد کاملاً متوقف شود
        if (dist < 0.4f && !arrived)
        {
            arrived = true;
            isDepositing = false;

            rb.position = depositTarget;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;

            LevelManager.Instance.BallDeposited(this);
        }

        return;
    }

    // ================= STACK =================
    if (!IsCollected) return;

    Vector3 target = LevelManager.Instance.GetBallPosition(stackIndex);
    Vector3 dir2 = target - rb.position;

    rb.AddForce(dir2 * 40f, ForceMode.Acceleration);

    // محدود کردن سرعت استک
    if (rb.velocity.magnitude > 8f)
        rb.velocity = rb.velocity.normalized * 8f;
}
}