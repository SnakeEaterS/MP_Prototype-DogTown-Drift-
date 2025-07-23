using System.Collections;
using UnityEngine;

public class MoveIndicator : MonoBehaviour
{
    [Header("Move/Bob Settings")]
    public GameObject targetObject;      // The object to move
    public float moveDuration = 3f;      // How long to bob before disabling
    public float moveAmplitude = 0.5f;   // How high up/down it moves
    public float moveSpeed = 2f;         // How fast it bobs

    private Vector3 initialPosition;

    void Awake()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("No target object assigned to MoveIndicator.");
            return;
        }

        // Cache the starting position
        initialPosition = targetObject.transform.localPosition;
    }

    void OnEnable()
    {
        if (targetObject != null)
            StartCoroutine(MoveAndDisable());
    }

    IEnumerator MoveAndDisable()
    {
        float timer = 0f;

        while (timer < moveDuration)
        {
            float offsetY = Mathf.Sin(Time.time * moveSpeed) * moveAmplitude;
            Vector3 newPosition = initialPosition + Vector3.up * offsetY;
            targetObject.transform.localPosition = newPosition;

            yield return null;
            timer += Time.deltaTime;
        }

        // Reset position before disabling if you like
        targetObject.transform.localPosition = initialPosition;

        targetObject.SetActive(false);
    }
}
