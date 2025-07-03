using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraMoverAndSceneLoader : MonoBehaviour
{
    public Transform cameraTarget;           // Target position & rotation for the camera
    public float duration = 2f;              // Time it takes to move
    public string nextSceneName;             // Name of the next scene to load
    public GameObject canvasToHide;          // The Canvas to hide on button click
    public GameObject motorbike;

    private Transform cam;
    private Vector3 startPos;
    private Quaternion startRot;
    private float timer = 0f;
    private bool isMoving = false;

    void Start()
    {
        cam = Camera.main.transform;
        if (motorbike != null)
        {
            motorbike.transform.SetParent(cam);
            //motorbike.transform.localPosition = new Vector3(0, 0f, 0f); // Adjust to suit your view
            //motorbike.transform.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        if (isMoving)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            // Ease in-out
            t = t * t * (3f - 2f * t);

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
        if (canvasToHide != null)
        {
            Debug.Log("Hiding canvas: " + canvasToHide.name);
            canvasToHide.SetActive(false); // Hide the whole UI
        }

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
