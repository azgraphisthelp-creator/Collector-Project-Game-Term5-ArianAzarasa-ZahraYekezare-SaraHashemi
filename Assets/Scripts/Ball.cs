using UnityEngine;

public class Ball : MonoBehaviour
{
    public bool InFinishMode { get; private set; }
    public bool IsCollected { get; set; }


    private Rigidbody rb;
    private Renderer rend;

    private int stackIndex;


    [Header("Stack Movement")]
    [SerializeField] private float stackMoveSpeed = 18f;
    [SerializeField] private float rotationDamping = 10f;

    [Header("Deposit Movement")]
    [SerializeField] private float depositSpeed = 15f;


    private bool isDepositing;
    private Vector3 depositTarget;

    private bool arrived;


    private Collider col;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rend = GetComponentInChildren<Renderer>();

        SetRandomColor();
    }



    private void SetRandomColor()
    {
        if (rend == null)
            return;

        rend.material.color = Random.ColorHSV();
    }



    public void SetStackIndex(int index)
    {
        stackIndex = index;
    }



    public void EnterStackMode()
    {
        IsCollected = true;

        isDepositing = false;


        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;


        rb.useGravity = false;
        rb.isKinematic = true;


        if(col != null)
            col.enabled = false;
    }



    public void GoToDeposit(Vector3 target)
    {
        isDepositing = true;

        depositTarget = target;

        IsCollected = false;

        arrived = false;


        rb.isKinematic = false;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;


        rb.useGravity = false;


        if(col != null)
            col.enabled = true;
    }



    private void FixedUpdate()
    {
        if(rb == null)
            return;


        if(InFinishMode)
            return;



        if(isDepositing)
        {

            float dist =
                Vector3.Distance(
                    rb.position,
                    depositTarget
                );


            if(dist < 0.5f && !arrived)
            {
                arrived = true;

                isDepositing = false;


                rb.position = depositTarget;


                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;


                rb.useGravity = true;


                LevelManager.Instance.BallDeposited(this);


                return;
            }



            rb.MovePosition(
                Vector3.MoveTowards(
                    rb.position,
                    depositTarget,
                    depositSpeed * Time.fixedDeltaTime
                )
            );


            return;
        }





        if(IsCollected)
        {

            Vector3 target =
                LevelManager.Instance.GetBallPosition(stackIndex);



            transform.position =
                Vector3.Lerp(
                    transform.position,
                    target,
                    Time.fixedDeltaTime * stackMoveSpeed
                );



            transform.rotation =
                Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.identity,
                    Time.fixedDeltaTime * rotationDamping
                );

        }

    }



    public void EnterFinishMode()
    {
        InFinishMode = true;


        IsCollected = false;

        isDepositing = false;


        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;


        rb.isKinematic = true;
        rb.useGravity = false;


        if(col != null)
            col.enabled = false;
    }
}