using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class CameraMoverAndSceneLoader : MonoBehaviour
{
    public Transform cameraTarget;
    public float duration = 2f;
    public string nextSceneName;
    public List<GameObject> buttonsToHide;
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
        }
    }

    void Update()
    {
        if (isMoving)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            t = t * t * (3f - 2f * t); // Ease in-out

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
        if (buttonsToHide != null && buttonsToHide.Count > 0)
        {
            int finishedTweens = 0;

            foreach (GameObject btn in buttonsToHide)
            {
                RectTransform rect = btn.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.DOAnchorPosX(2600f, 0.5f).SetEase(Ease.InOutCubic).OnComplete(() =>
                    {
                        btn.gameObject.SetActive(false);
                        finishedTweens++;

                        // After all buttons are hidden, start camera move
                        if (finishedTweens == buttonsToHide.Count)
                        {
                            BeginCameraMovement();
                        }
                    });
                }
                else
                {
                    btn.gameObject.SetActive(false);
                    finishedTweens++;

                    if (finishedTweens == buttonsToHide.Count)
                    {
                        BeginCameraMovement();
                    }
                }
            }
        }
        else
        {
            // No buttons, start camera immediately
            BeginCameraMovement();
        }
    }

    private void BeginCameraMovement()
    {
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
