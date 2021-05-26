using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class freefall : MonoBehaviour
{
    private float v;
    private GameObject human;

    // Start is called before the first frame update
    void Start()
    {
        human = gameObject;
        v = 0;
    }

    // Update is called once per frame
    void Update()
    {
        v += Time.time * Mathf.Pow(10, -5);
        human.transform.Translate(0, v, 0);
    }
}
