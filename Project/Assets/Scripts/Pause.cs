using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public static bool pause = false;
    private bool diconnecting = false;

    public void TogglePause()
    {
        if(diconnecting)
        {
            return;
        }

        pause = !pause;

        transform.GetChild(0).gameObject.SetActive(pause);
        Cursor.lockState = (pause) ? CursorLockMode.None : CursorLockMode.Confined;
        Cursor.visible = pause;

    }

    public void Quit()
    {
        diconnecting = true;
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

 
}
