using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;


    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;


    private int score = 0;



    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }



    private void Start()
    {
        UpdateUI();
    }



    public void AddScore(int amount)
    {
        score += amount;

        UpdateUI();

        Debug.Log("Score : " + score);
    }



    public int GetScore()
    {
        return score;
    }



    private void UpdateUI()
    {
        if(scoreText != null)
        {
            scoreText.text = "Score : " + score;
        }
    }
}