using UnityEngine;

public class FinishButton : MonoBehaviour
{
    public void ClickFinish()
    {
        LevelManager.Instance.FinishButtonPressed();

        gameObject.SetActive(false);
    }
}