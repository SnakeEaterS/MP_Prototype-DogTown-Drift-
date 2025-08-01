using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class CameraMoverAndSceneLoader : MonoBehaviour
{
    [Header("Camera & Scene")]
    public Transform cameraTarget;
    public float duration = 2f;
    public string nextSceneName;

    [Header("UI")]
    public List<GameObject> buttonsToHide;
    public Image blackScreen; // Assign a full-screen black UI Image in Inspector
    public float fadeDuration = 1f;

    [Header("Motorbike")]
    public GameObject motorbike;
    public Vector3 motorbikeTargetEuler = Vector3.zero; // Final upright rotation in Euler angles
    public float motorbikeRotationDuration = 1f;

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

        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
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

            if (blackScreen != null)
            {
                blackScreen.color = new Color(0, 0, 0, t);  // Fade alpha from 0 to 1
            }

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

                        if (finishedTweens == buttonsToHide.Count)
                        {
                            StartMotorbikeAnimation();
                        }
                    });
                }
                else
                {
                    btn.gameObject.SetActive(false);
                    finishedTweens++;

                    if (finishedTweens == buttonsToHide.Count)
                    {
                        StartMotorbikeAnimation();
                    }
                }
            }
        }
        else
        {
            StartMotorbikeAnimation();
        }
    }

    private void StartMotorbikeAnimation()
    {
        if (motorbike != null)
        {
            motorbike.transform.DOLocalRotate(motorbikeTargetEuler, motorbikeRotationDuration)
                .SetEase(Ease.InOutCubic)
                .OnComplete(BeginCameraMovement);
        }
        else
        {
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
