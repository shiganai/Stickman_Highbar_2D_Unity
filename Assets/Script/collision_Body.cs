using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collision_Body : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("“–‚½‚Á‚½‚æ");

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("“–‚½‚Á‚½‚æ");
    }
}
