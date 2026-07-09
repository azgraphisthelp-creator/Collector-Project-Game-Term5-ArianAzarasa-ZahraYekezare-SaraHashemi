using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FinishZone : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject swipeUpPanel;

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


        playerReached = true;


        if (swipeUpPanel != null)
            swipeUpPanel.SetActive(true);


        PlayerMovement movement = other.GetComponent<PlayerMovement>();

        if (movement != null)
            movement.StopMovement();



        ballsToSend = new List<Ball>(
            LevelManager.Instance.collectedBalls
        );


        Debug.Log("Finish Reached");
    }





    private void Update()
    {
        if (!playerReached)
            return;



        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);


            if(touch.phase == TouchPhase.Began)
            {
                startTouch = touch.position;
            }


            if(touch.phase == TouchPhase.Ended)
            {
                if(touch.position.y - startTouch.y > swipeDistance)
                {
                    SendBall();
                }
            }
        }



#if UNITY_EDITOR

        if(Input.GetKeyDown(KeyCode.Space))
        {
            SendBall();
        }

#endif
    }





    private void SendBall()
    {
        if (swipeUpPanel != null)
            swipeUpPanel.SetActive(false);



        if(ballsToSend == null)
            return;


        if(currentBallIndex >= ballsToSend.Count)
            return;



        StartCoroutine(SendBallRoutine());


        currentBallIndex++;
    }







    IEnumerator SendBallRoutine()
    {
        int index = currentBallIndex;


        Ball ball = ballsToSend[index];


        if(ball == null)
            yield break;



        LevelManager.Instance.collectedBalls.Remove(ball);


        ScoreManager.Instance.AddScore(10);



        UIManager.Instance.SetBallText(
            LevelManager.Instance.collectedBalls.Count
        );



        Vector3 targetPosition =
            targetPoint.position +
            new Vector3(
                (index % 3 - 1) * 0.7f,
                0,
                -(index / 3) * 0.7f
            );



        ball.transform.SetParent(null);



        Rigidbody rb = ball.GetComponent<Rigidbody>();

        if(rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }





        TrailRenderer trail = ball.GetComponent<TrailRenderer>();

        if(trail != null)
        {
            trail.Clear();
            trail.enabled = true;
        }





        ball.transform
            .DOMove(targetPosition,0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {

                ball.transform.SetParent(targetPoint);
                ball.transform.position = targetPosition;



                if(trail != null)
                {
                    StartCoroutine(DisableTrail(trail));
                }



                finishedBalls++;



                if(finishedBalls >= ballsToSend.Count)
                {
                    PlayFinishEffect();
                }

            });




        yield return new WaitForSeconds(sendDelay);



        LevelManager.Instance.RefreshStackPublic();
    }








    private void PlayFinishEffect()
    {
        if(finishPlayed)
            return;


        finishPlayed = true;



        Debug.Log("🎆 FIREWORK COMPLETE");



        if(fireworkFX != null)
        {
            fireworkFX.transform.position = targetPoint.position;
            fireworkFX.Play();
        }



        if(audioSource != null && fireworkSound != null)
        {
            audioSource.PlayOneShot(fireworkSound);
        }
    }







    IEnumerator DisableTrail(TrailRenderer trail)
    {
        yield return new WaitForSeconds(trailDisableDelay);


        trail.Clear();
        trail.enabled = false;
    }
}