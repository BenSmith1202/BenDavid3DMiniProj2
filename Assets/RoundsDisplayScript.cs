using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundsDisplayScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TMP_Text text = GetComponent<TMP_Text>();
        
        int roundsSurvived = PlayerPrefs.GetInt("rounds");
        
        text.SetText("You Survived " + roundsSurvived + " Rounds!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
