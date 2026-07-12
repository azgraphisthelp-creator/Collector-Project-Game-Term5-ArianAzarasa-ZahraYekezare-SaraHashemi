using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FinishZone : MonoBehaviour
{
    private bool sendingBall;
    [Header("UI")]


    [Header("Ball Target")]
    [SerializeField] private Transform targetPoint;


    [Header("Input")]
    [SerializeField] private float swipeDistance = 100f;


    [Header("Send Settings")]
    [SerializeField] private float sendDelay = 0.08f;


    [Header("Trail Settings")]
    [SerializeField] private float trailDisableDelay = 0.3f;

    [Header("Finish Celebration")]
    [SerializeField] private ParticleSystem fireworkFX;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireworkSound;



    private Vector2 startTouch;


    private bool playerReached;
    private bool finishPlayed;


    private int finishedBalls = 0;


    private List<Ball> ballsToSend;

    private int currentBallIndex = 0;



    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;


        if (playerReached)
            return;


        playerReached = true;



        if(UIManager.Instance != null)
{
    UIManager.Instance.ShowSwipePanel();
}



        PlayerMovement movement =
            other.GetComponent<PlayerMovement>();


        if (movement != null)
            movement.StopMovement();




        ballsToSend = new List<Ball>(
            LevelManager.Instance.collectedBalls
        );

Debug.Log(
    "BEFORE FINISH REMOVE = "
    + LevelManager.Instance.collectedBalls.Count
);

        Debug.Log(
            "FINISH REACHED - BALL COUNT : "
            + ballsToSend.Count
        );



        if (ballsToSend.Count == 0)
        {
            PlayFinishEffect();
        }
    }







    private void Update()
    {
        if (!playerReached)
            return;



        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);



            if (touch.phase == TouchPhase.Began)
            {
                startTouch = touch.position;
            }



            if (touch.phase == TouchPhase.Ended)
            {
                if (touch.position.y - startTouch.y > swipeDistance)
                {
                    SendBall();
                }
            }
        }



#if UNITY_EDITOR

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendBall();
        }

#endif
    }






private void SendBall()
{
    if (sendingBall)
        return;


    if (ballsToSend == null)
        return;


    if (currentBallIndex >= ballsToSend.Count)
        return;


    


    sendingBall = true;


    StartCoroutine(SendBallRoutine());
}

IEnumerator SendBallRoutine()
{
    if(currentBallIndex >= ballsToSend.Count)
        yield break;
    int index = currentBallIndex;
    currentBallIndex++;


    Ball ball = ballsToSend[index];


    if (ball == null)
{
    sendingBall = false;
    yield break;
}



    ball.EnterFinishMode();
    LevelManager.Instance.RemoveBallFromFinish(ball);


if(UIManager.Instance != null)
{
    UIManager.Instance.SetBallText(
        LevelManager.Instance.GetBallCount()
    );
}
    ball.transform.SetParent(null);



    Vector3 targetPosition =
        targetPoint.position +
        new Vector3(
            (index % 3 - 1) * 0.7f,
            0,
            -(index / 3) * 0.7f
        );



    TrailRenderer trail =
        ball.GetComponent<TrailRenderer>();


    if(trail != null)
    {
        trail.Clear();
        trail.enabled = true;
    }



    ball.transform
        .DOMove(
            targetPosition,
            0.5f
        )
        .SetEase(Ease.OutBack)
        .OnComplete(() =>
        {

            ball.transform.SetParent(targetPoint);

            ball.transform.position = targetPosition;



            if(trail != null)
            {
                StartCoroutine(
                    DisableTrail(trail)
                );
            }



            finishedBalls++;
            if (ScoreManager.Instance != null)
{
    ScoreManager.Instance.AddScore(10);
}


            Debug.Log(
                "FINISHED BALL : "
                + finishedBalls
                + "/" 
                + ballsToSend.Count
            );



if(finishedBalls >= ballsToSend.Count)
{
    Debug.Log("ALL FINISH BALLS ARRIVED");

    LevelManager.Instance.RefreshStackPublic();

    UIManager.Instance.SetBallText(
        LevelManager.Instance.GetBallCount()
    );

    PlayFinishEffect();
}
        });



    yield return null;

    sendingBall = false;
}



    IEnumerator DisableTrail(TrailRenderer trail)
    {
        yield return new WaitForSeconds(
            trailDisableDelay
        );


        trail.Clear();

        trail.enabled = false;
    }
    
private void PlayFinishEffect()
{
    if (finishPlayed)
        return;

    finishPlayed = true;

    Debug.Log("🎆 FINISH COMPLETE");


    if (fireworkFX != null)
    {
        fireworkFX.transform.position = targetPoint.position;
        fireworkFX.Play();
    }


    if (audioSource != null && fireworkSound != null)
    {
        audioSource.PlayOneShot(fireworkSound);
    }


    StartCoroutine(ShowPanelDelay());
}



private IEnumerator ShowPanelDelay()
{
    yield return new WaitForSeconds(2f);

    ShowEndPanel();
}
private void ShowEndPanel()
{
    if (UIManager.Instance != null)
    {
        UIManager.Instance.ShowFinishPanel();
    }
}

}