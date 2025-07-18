using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class GameOver : MonoBehaviour
{
    public Text scoreText;
    public Text highScoreText;
    public Image fadeImage; // Assign in Inspector
    public float animationDuration = 1.5f;
    public string sceneNameToLoad = "MainMenu";
    public float waitBeforeLoad = 1.0f;
    public float fadeDuration = 1.0f;

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

        // Optional pause
        yield return new WaitForSeconds(waitBeforeLoad);

        // Fade to black
        yield return fadeImage.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad).WaitForCompletion();

        // Load next scene
        SceneManager.LoadScene(sceneNameToLoad);
    }
}
