using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBarScript : MonoBehaviour
{
    // Start is called before the first frame update
    Slider slider;

    public void SetSliderValue(float value)
    {
        value = Mathf.Clamp(value, slider.minValue, slider.maxValue);
        slider.value = value;
    }

    
}
