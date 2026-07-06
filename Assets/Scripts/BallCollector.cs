using UnityEngine;

public class BallCollector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball"))
            return;

        Ball ball =
            other.GetComponentInParent<Ball>();

        if (ball == null)
            return;

        if (ball.IsCollected)
            return;

        ball.IsCollected = true;

        LevelManager.Instance.AddBall(ball);
    }
}