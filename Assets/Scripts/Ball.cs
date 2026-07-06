using UnityEngine;

public class Ball : MonoBehaviour
{
    public bool IsCollected { get; set; }

    private Rigidbody rb;
    private Renderer rend;

    private int stackIndex;

    private bool isDepositing;
    private Vector3 depositTarget;

    private bool deposited;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            rb = GetComponentInParent<Rigidbody>();

        rend = GetComponentInChildren<Renderer>();

        SetRandomColor();
    }

    private void SetRandomColor()
    {
        if (rend == null)
            return;

        rend.material.color = Random.ColorHSV(
            0f, 1f,
            0.6f, 1f,
            0.8f, 1f
        );
    }

    public void SetStackIndex(int index)
    {
        stackIndex = index;
    }

    public void GoToDeposit(Vector3 target)
    {
        deposited = false;
        isDepositing = true;

        IsCollected = false;

        depositTarget = target;
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        //---------------- Deposit ----------------

        if (isDepositing)
        {
            Vector3 dir = depositTarget - rb.position;

            if (dir.sqrMagnitude > 0.0001f)
                rb.velocity = dir.normalized * 10f;

            if (!deposited && dir.magnitude < 0.25f)
            {
                deposited = true;
                isDepositing = false;

                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                LevelManager.Instance.BallDeposited(this);
            }

            return;
        }

        //---------------- Stack ----------------

        if (!IsCollected)
            return;

        Vector3 target =
            LevelManager.Instance.GetBallPosition(stackIndex);

        Vector3 force =
            (target - rb.position) * 40f;

        rb.AddForce(force, ForceMode.Acceleration);
    }
}