using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{


    


    public void StartGame()
    {
        // Starts the game by loading the scene after showing a transition.

        SceneManager.LoadScene(1);  
    }

    public void GoHome()
    {
        // Exits the game.
        SceneManager.LoadScene(0); 
    }
    public void ExitGame()
    {
        // Exits the game.
        Application.Quit();
    }

}

