using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiFunctions : MonoBehaviour
{   

    int stepCount = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void TickStepCount()
    {
        stepCount++;
        gameObject.transform.Find("StepCount").GetComponent<Text>().text = stepCount.ToString();
    }

    public void SetTreeCount()
    {  
        int treeCount = GameObject.Find("Ground").GetComponent<GroundFunctions>().getTreeCount();
        gameObject.transform.Find("TreeCount").GetComponent<Text>().text = treeCount.ToString();
    }

    public float GetRain(){
        return gameObject.transform.Find("RainSlider").GetComponent<Slider>().value;
    }

    public bool IsRaining(){
        return gameObject.transform.Find("RainToggle").GetComponent<Toggle>().isOn;
    }
}
