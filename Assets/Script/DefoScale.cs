
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 
public class DefoScale : MonoBehaviour
{
    public Slider slider;
    public GameObject obj;
 
    public void ScaleAdjust()
    {   
        if(obj != null){
        float scale = slider.value;
        obj.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public void ShowSlider(){
        slider.gameObject.SetActive(true);
    }

    public void HideSlider(){
        slider.gameObject.SetActive(false);
    }
}