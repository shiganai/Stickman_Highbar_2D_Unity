using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collision_Control_Main : BaseCollisionControl
{
    protected override void OnTriggerEnter2D_Custom(string objectName, Collider2D collision)
    {
        if ("Body".Equals(objectName))
        {
            this.collision_Body(collision);
        }
        else
        {
            throw new System.Exception("Not implemented!!");
        }
    }
    private void collision_Body(Collider2D collision)
    {
        Debug.Log("Collison on Body");
    }
}
