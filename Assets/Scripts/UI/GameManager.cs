using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int playerScore = 0;
    public TextMeshProUGUI scoreText;
    public int partsDestroyed = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);  
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }



    private void Update()
    {
        if (partsDestroyed == 2)
        {
            Debug.Log("All parts destroyed, player can die now.");
            StartCoroutine(EndGame());
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

    private IEnumerator EndGame()
    {
        // animation for the boss death

        yield return new WaitForSeconds(10f);
        WinningScreen();
    }

    public void WinningScreen()
    {
        Debug.Log("Player has won the game");
        SceneManager.LoadScene("Score");
    }
}
