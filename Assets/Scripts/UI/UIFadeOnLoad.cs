using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIFadeOnLoad : MonoBehaviour
{
    public Image uiImage;             // Drag your UI Image here
    public float fadeDuration = 2f;   // Duration of the fade

    void Start()
    {
        if (uiImage == null)
            uiImage = GetComponent<Image>();

        // Start fully opaque
        Color color = uiImage.color;
        color.a = 1f;
        uiImage.color = color;

        // Fade to alpha 0
        uiImage.DOFade(0f, fadeDuration);
    }
}
