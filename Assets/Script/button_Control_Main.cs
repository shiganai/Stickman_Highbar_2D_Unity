using System.Collections;
using UnityEngine;

public class button_Control_Main : BaseButtonController
{
    private GameObject human;
    private freefall freefall_Cs;
    private void Start()
    {
        human = GameObject.Find("Human");
        freefall_Cs = human.GetComponent<freefall>();
    }
    protected override void OnClick(string objectName)
    {
        // オブジェクトの数だけ処理を分岐
        if ("TakeOff".Equals(objectName))
        {
            this.TakeOff();
        }
        else if ("Reset".Equals(objectName))
        {
            this.Reset();
        }
        else
        {
            throw new System.Exception("Not implemented!!");
        }
    }

    private void TakeOff()
    {
        Debug.Log("TakeOff Click");
        freefall_Cs.Set_Status_Inair();
    }

    private void Reset()
    {
        Debug.Log("Reset Click");
        freefall_Cs.Initialized();
        ;
    }

    private void Button2Click()
    {
        Debug.Log("Button2 Click");
    }
}