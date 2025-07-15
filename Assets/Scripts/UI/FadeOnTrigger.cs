using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeOnTrigger : MonoBehaviour
{
    public Image fadeImage;          // Assign in Inspector
    public float fadeDuration = 1f;  // Time to fully fade

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Make sure your player has the tag
        {
            FadeToBlack();
        }
    }

    void FadeToBlack()
    {
        fadeImage.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad);
    }
}
