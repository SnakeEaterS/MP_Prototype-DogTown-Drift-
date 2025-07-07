using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class GameOver : MonoBehaviour
{
    public Text scoreText;
    public Text highScoreText;
    public float animationDuration = 1.5f;
    public string sceneNameToLoad = "MainMenu"; // Set this in Inspector or change it here
    public float waitBeforeLoad = 1.0f; // Extra delay after animation before scene loads

    private void Start()
    {
        StartCoroutine(AnimateScoresAndLoadScene());
    }

    private IEnumerator AnimateScoresAndLoadScene()
    {
        int playerScore = PlayerPrefs.GetInt("PlayerScore", 0);
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        int displayScore = 0;
        int displayHighScore = 0;

        // Animate player score
        Tween scoreTween = DOTween.To(() => displayScore, x =>
        {
            displayScore = x;
            scoreText.text = "Your Score: " + displayScore.ToString("N0");
        }, playerScore, animationDuration).SetEase(Ease.OutCubic);

        yield return scoreTween.WaitForCompletion();

        // Animate high score
        Tween highScoreTween = DOTween.To(() => displayHighScore, x =>
        {
            displayHighScore = x;
            highScoreText.text = "High Score: " + displayHighScore.ToString("N0");
        }, highScore, animationDuration).SetEase(Ease.OutCubic);

        yield return highScoreTween.WaitForCompletion();

        // Optional pause before loading new scene
        yield return new WaitForSeconds(waitBeforeLoad);

        // Load next scene
        SceneManager.LoadScene(sceneNameToLoad);
    }
}
