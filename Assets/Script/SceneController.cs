using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void ChangeScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
