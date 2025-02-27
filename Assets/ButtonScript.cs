using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{

    public bool isStartButton; //else end button
    // Start is called before the first frame update
    public void OnPress()
    {
        if (isStartButton)
        {
            SceneManager.LoadScene(1);
        } else
        {
            Application.Quit();
        }
    }
}
