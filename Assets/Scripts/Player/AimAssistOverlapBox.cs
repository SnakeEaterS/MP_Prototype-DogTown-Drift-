using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AimAssistOverlapBox : MonoBehaviour
{
    public RectTransform crosshairUI;
    public Canvas canvas;
    public LayerMask enemyLayer;

    public BoxCollider assistBox; // Reference to scene object!

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (assistBox == null) 
        { 
            Debug.LogWarning("Assist Box is not assigned in AimAssistOverlapBox script.");
            return; 
        }

        Vector3 boxCenter = assistBox.transform.position;
        Vector3 boxSize = assistBox.size;
        Quaternion boxRotation = assistBox.transform.rotation;

        // Convert local size to world size if scaled
        boxSize = Vector3.Scale(boxSize, assistBox.transform.lossyScale);

        Collider[] hits = Physics.OverlapBox(boxCenter, boxSize / 2f, boxRotation, enemyLayer);

        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy") || hit.CompareTag("EnemyHead"))
            {
                float distance = Vector3.Distance(mainCamera.transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = hit.transform;
                }
            }
        }

        Vector2 targetScreenPos = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (bestTarget != null)
        {
            targetScreenPos = mainCamera.WorldToScreenPoint(bestTarget.position);
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            targetScreenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out Vector2 uiPos
        );

        crosshairUI.anchoredPosition = Vector2.Lerp(
            crosshairUI.anchoredPosition,
            uiPos,
            Time.deltaTime * 10f
        );
    }

    void OnDrawGizmos()
    {
        if (assistBox == null) return;

        Gizmos.color = Color.green;
        Gizmos.matrix = assistBox.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, assistBox.size);
    }
}