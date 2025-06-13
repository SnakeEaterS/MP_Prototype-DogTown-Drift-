using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int playerScore = 0;
    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int score)
    {
        playerScore += score;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText == null)
        {
            // Try to find the score text again in case it was lost during scene load
            scoreText = GameObject.FindWithTag("ScoreText")?.GetComponent<TextMeshProUGUI>();
        }

        if (scoreText != null)
        {
            scoreText.text = playerScore.ToString();
        }
    }

    public void PlayerDie()
    {
        Debug.Log("Player has died");

        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (playerScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", playerScore);
        }

        PlayerPrefs.SetInt("PlayerScore", playerScore);
        PlayerPrefs.Save();

        SceneManager.LoadScene(4);
    }

    public void ResetScore()
    {
        playerScore = 0;
        UpdateScoreUI();
    }
}
