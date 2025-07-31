using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayButtonFlasher : MonoBehaviour
{
    public Image glowImage; // Assign this in the Inspector

    void Start()
    {
        // Ensure the image starts fully transparent
        Color color = glowImage.color;
        color.a = 0f;
        glowImage.color = color;

        // Loop fade in and out (flash effect)
        glowImage.DOFade(1f, 0.7f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}
