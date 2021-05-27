using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class freefall : MonoBehaviour
{
    private float v;
    private GameObject human;
    private GameObject body;

    // Start is called before the first frame update
    void Start()
    {
        human = gameObject;
        body = GameObject.Find("Body");
        v = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        v += Time.fixedDeltaTime * Mathf.Pow(10, -1.5f);
        //human.transform.Translate(0, v, 0);
        human.transform.Rotate(0, 0, Time.fixedDeltaTime * 3 * Mathf.Pow(10, 1f));
    }
}
