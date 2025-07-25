using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public TextMeshProUGUI scoreText;

    public int playerScore = 0;
    public int partsDestroyed = 0;
    public bool didWin = false;
    public bool secondPhaseStart = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        UpdateScoreUI();
        if (partsDestroyed == 1 && secondPhaseStart == false)
        {
            DroneBossFollower droneBossFollower = FindObjectOfType<DroneBossFollower>();
            droneBossFollower.SecondPhase();
            secondPhaseStart = true;
        }
        if (partsDestroyed == 2)
        {
            Debug.Log("All parts destroyed, player can die now.");
            StartCoroutine(EndGame());
        }
    }

    public void AddScore(int score)
    {
        playerScore += score;
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
        didWin = false;
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
        partsDestroyed = 0; 
        
        yield return new WaitForSeconds(1f);
        WinningScreen();
    }

    public void WinningScreen()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (playerScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", playerScore);
        }

        PlayerPrefs.SetInt("PlayerScore", playerScore);
        PlayerPrefs.Save();
        Debug.Log("Player has won the game");
        didWin = true;
        SceneManager.LoadScene("End");
    }
}
