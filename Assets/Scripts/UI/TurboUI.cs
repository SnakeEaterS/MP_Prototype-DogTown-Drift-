using UnityEngine;
using UnityEngine.UI;

public class TurboUI : MonoBehaviour
{
    public Image turboBarFill; // Drag TurboBarFill here in Inspector
    public JoyconRevController controller; // Reference to your controller script

    [Header("Smooth Fill")]
    public float fillLerpSpeed = 2f; // Adjust for how fast the bar catches up

    void Update()
    {
        if (controller != null && turboBarFill != null)
        {
            float normalizedCharge = controller.GetTurboChargeNormalized();

            // Smoothly interpolate the fill amount toward the actual charge
            turboBarFill.fillAmount = Mathf.Lerp(
                turboBarFill.fillAmount,
                normalizedCharge,
                Time.deltaTime * fillLerpSpeed
            );
        }
    }
}
