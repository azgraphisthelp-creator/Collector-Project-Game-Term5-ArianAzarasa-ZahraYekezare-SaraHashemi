using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;


    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;


    private int score = 0;
    private int highScore = 0;



    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;


        highScore = PlayerPrefs.GetInt(
            "HIGH_SCORE",
            0
        );
    }



    private void Start()
    {
        UpdateUI();
    }



    public void AddScore(int amount)
    {
        score += amount;


        CheckHighScore();


        UpdateUI();


        Debug.Log(
            "Score : " + score
        );
    }



    private void CheckHighScore()
    {
        if(score > highScore)
        {
            highScore = score;


            PlayerPrefs.SetInt(
                "HIGH_SCORE",
                highScore
            );


            PlayerPrefs.Save();


            Debug.Log(
                "NEW HIGH SCORE : " + highScore
            );
        }
    }



    public int GetScore()
    {
        return score;
    }



    public int GetHighScore()
    {
        return highScore;
    }



    public void ResetScore()
    {
        score = 0;

        UpdateUI();
    }



    public void SaveFinalScore()
    {
        CheckHighScore();
    }



    private void UpdateUI()
    {
        if(scoreText != null)
        {
            scoreText.text =
                "Score : " + score;
        }


        if(highScoreText != null)
        {
            highScoreText.text =
                "Best : " + highScore;
        }
    }
}