using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonSlideIn : MonoBehaviour
{
    public RectTransform[] buttons; // Assign in Inspector (order matters)
    public float slideDuration = 0.5f;
    public float delayBetween = 0.2f;
    public float offsetX = 300f; // Distance to start offscreen

    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            RectTransform btn = buttons[i];

            // Move them off-screen to the right
            Vector2 originalPos = btn.anchoredPosition;
            btn.anchoredPosition += new Vector2(offsetX, 0);

            // Slide them in one by one
            btn.DOAnchorPos(originalPos, slideDuration)
                .SetEase(Ease.OutCubic)
                .SetDelay(i * delayBetween);
        }
    }
}
