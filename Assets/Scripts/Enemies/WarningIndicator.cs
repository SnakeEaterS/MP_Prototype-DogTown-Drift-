using UnityEngine;

public class WarningZone : MonoBehaviour
{
    [HideInInspector] public BossAttacks bossAttacks;

    void OnTriggerEnter(Collider other)
    {
        // Use CompareTag so you only check for player-related colliders
        if (other.CompareTag("Player"))
        {
            bossAttacks.PlayerEnteredWarningZone();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bossAttacks.PlayerExitedWarningZone();
        }
    }
}
