using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCollisionControl : MonoBehaviour
{
    public BaseCollisionControl target;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        this.OnTriggerEnter2D_Custom(collision);
    }

    public void OnTriggerEnter2D_Custom(Collider2D collision)
    {
        if (target == null)
        {
            throw new System.Exception("target instance is null!!");
        }
        else
        {
            target.OnTriggerEnter2D_Custom(this.gameObject.name, collision);
        }
    }

    protected virtual void OnTriggerEnter2D_Custom(string objectName, Collider2D collision)
    {
        Debug.Log("Base Button");   // í èÌåƒÇŒÇÍÇ»Ç¢
    }
}
