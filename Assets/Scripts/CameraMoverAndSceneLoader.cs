using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CameraMoverAndSceneLoader : MonoBehaviour
{
    public Transform cameraTarget;           // Target position & rotation
    public float duration = 2f;              // Movement duration
    public string nextSceneName;             // Scene to load
    public GameObject buttonToHide;          // UI Button to hide

    private Transform cam;
    private Vector3 startPos;
    private Quaternion startRot;
    private float timer = 0f;
    private bool isMoving = false;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        if (isMoving)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            // Smoothstep easing
            t = t * t * (3f - 2f * t);

            // Move and rotate camera
            cam.position = Vector3.Lerp(startPos, cameraTarget.position, t);
            cam.rotation = Quaternion.Slerp(startRot, cameraTarget.rotation, t);

            if (t >= 1f)
            {
                isMoving = false;
                LoadNextScene();
            }
        }
    }

    public void StartCameraMove()
    {
        if (buttonToHide != null)
            buttonToHide.SetActive(false); // Hide button

        startPos = cam.position;
        startRot = cam.rotation;
        timer = 0f;
        isMoving = true;
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
