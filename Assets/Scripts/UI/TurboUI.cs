using UnityEngine;
using UnityEngine.UI;

public class TurboUI : MonoBehaviour
{
    public Image turboBarFill; // Drag TurboBarFill here in Inspector
    public JoyconRevController controller; // Reference to your controller script

    void Update()
    {
        if (controller != null && turboBarFill != null)
        {
            float normalizedCharge = controller.GetTurboChargeNormalized();
            turboBarFill.fillAmount = normalizedCharge;
        }
    }
}
