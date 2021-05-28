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
        // �I�u�W�F�N�g�̐����������𕪊�
        if ("TakeOff".Equals(objectName))
        {
            this.TakeOff();
        }
        else
        {
            throw new System.Exception("Not implemented!!");
        }
    }

    private void TakeOff()
    {
        Debug.Log("TakeOff Click");
        freefall_Cs.status = "Inair";
    }

    private void Button2Click()
    {
        Debug.Log("Button2 Click");
    }
}