using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

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
            DontDestroyOnLoad(gameObject);
        }
        else
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
        if (scoreText != null)
        {
            scoreText.text = playerScore.ToString();

        }
    }

    public void PlayerDie()
    {
        Debug.Log("Player has died"); // Log a message for debugging purposes

        // Retrieve the saved high score from PlayerPrefs (default is 0)
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        // If the player's current score exceeds the high score, update it
        if (playerScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", playerScore); // Save the new high score
            PlayerPrefs.Save(); // Ensure the new high score is written to disk
        }

        // Save the player's current score to PlayerPrefs
        PlayerPrefs.SetInt("PlayerScore", playerScore);
        PlayerPrefs.Save(); // Persist the player's score to disk

        // Load the Game Over scene 
        SceneManager.LoadScene(4);
    }
}
