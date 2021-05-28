using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collision_Control_Main : BaseCollisionControl
{
    private GameObject human;
    private freefall freefall_Cs;
    private void Start()
    {
        human = GameObject.Find("Human");
        freefall_Cs = human.GetComponent<freefall>();
    }
    protected override void OnTriggerEnter2D_Custom(string objectName, Collider2D collision)
    {
        if ("Body".Equals(objectName))
        {
            this.collision_Body(collision);
        }
        else if ("Wrist".Equals(objectName))
        {
            this.collision_Wrist(collision);
        }
        else
        {
            throw new System.Exception("Not implemented!!");
        }
    }
    private void collision_Body(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("High_Bar"))
        {
            Debug.Log("Collison on Body");
            if (!freefall_Cs.Get_Status().Equals("onbar"))
            {
                freefall_Cs.Set_Status_Catch();
            }
        }
    }
    private void collision_Wrist(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("High_Bar"))
        {
            Debug.Log("Collison on Wrist");
            if (!freefall_Cs.Get_Status().Equals("onbar"))
            {
                freefall_Cs.Set_Status_Onbar();
            }
        }
    }
}
