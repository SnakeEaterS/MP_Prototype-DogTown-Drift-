using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For working with UI Text components

public class GameOver : MonoBehaviour
{
    // Reference to the Text component displaying the player's score
    public Text scoreText;

    // Reference to the Text component displaying the high score
    public Text highScoreText;

    void Start()
    {
        // Retrieve the player's score from PlayerPrefs (default to 0 if not found)
        int playerScore = PlayerPrefs.GetInt("PlayerScore", 0);

        // Retrieve the high score from PlayerPrefs (default to 0 if not found)
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        // Update the UI to display the player's score
        scoreText.text = "Your Score:" + playerScore.ToString();

        // Update the UI to display the high score
        highScoreText.text = "High Score:"+ highScore.ToString();

    }
}
