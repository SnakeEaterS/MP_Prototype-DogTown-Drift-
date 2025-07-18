using UnityEngine;

public class Windisplay : MonoBehaviour
{
    public GameObject winObject;
    public GameObject lossObject;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            bool didWin = GameManager.Instance.didWin;
            winObject.SetActive(didWin);
            lossObject.SetActive(!didWin);
        }
        else
        {
            Debug.LogWarning("GameManager instance not found.");
        }
    }
}
