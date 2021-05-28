using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class slider_Control : MonoBehaviour
{
    private Slider slider;
    private GameObject human;
    private freefall freefall_Cs;

    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        human = GameObject.Find("Human");
        freefall_Cs = human.GetComponent<freefall>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnValueChanged()
    {
        freefall_Cs.tau_Hip_Self = slider.value;
    }


}
