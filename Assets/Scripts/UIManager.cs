using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Texts")]
    [SerializeField] private TMP_Text ballText;
    [SerializeField] private TMP_Text levelText;

    [Header("Progress")]
    [SerializeField] private Slider progressSlider;

    [Header("Panels")]
    [SerializeField] private UIPanelAnimator losePanel;
    [SerializeField] private UIPanelAnimator pausePanel;

    [Header("UI Root")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private EventSystem eventSystem;

    private bool isPaused;
private void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (!isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }
}
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public void SetBallText(int count)
    {
        if (ballText != null)
            ballText.text = "Balls : " + count;
    }

    public void SetLevelText(int level)
    {
        if (levelText != null)
            levelText.text = "Level " + level;
    }

    public void UpdateProgress(float value)
    {
        if (progressSlider != null)
            progressSlider.value = value;
    }

   
    public bool CanInput()
    {
        return !isPaused;
    }


    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (pausePanel != null)
            pausePanel.Show();

        Time.timeScale = 0f;
        isPaused = true;

        SetUIInput(false);
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.Hide();

        Time.timeScale = 1f;
        isPaused = false;

        SetUIInput(true);
    }

 
    public void ShowLosePanel()
    {
        if (losePanel != null)
            losePanel.Show();

        Time.timeScale = 0f;

        isPaused = true;
        SetUIInput(false);
    }


    private void SetUIInput(bool enabled)
    {
        if (uiCanvas != null)
        {
            var raycaster = uiCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
                raycaster.enabled = enabled;
        }

        if (eventSystem != null)
            eventSystem.enabled = enabled;
    }
   public void QuitGame()
{
    StartCoroutine(QuitRoutine());
}

private IEnumerator QuitRoutine()
{
    yield return new WaitForSecondsRealtime(0.15f);

    Time.timeScale = 1f;

#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
}
}