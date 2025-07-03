using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CameraMoverAndSceneLoader : MonoBehaviour
{
    public Transform cameraTarget;           // Target position for the camera
    public float duration = 2f;              // Duration of the movement
    public string nextSceneName;             // Scene to load
    public GameObject buttonToHide;          // The UI Button to disable/hide

    private Transform cam;
    private Vector3 startPos;
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
            // Ease in-out (using smoothstep for nice camera easing)
            t = t * t * (3f - 2f * t);

            cam.position = Vector3.Lerp(startPos, cameraTarget.position, t);

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
            buttonToHide.SetActive(false); // Hide the button on click

        startPos = cam.position;
        timer = 0f;
        isMoving = true;
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
