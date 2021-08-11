using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    public MainMenu launcher;

    public void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void JoinMatch()
    {
        launcher.Join();
    }

    public void CreateMatch()
    {
        launcher.Create();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
