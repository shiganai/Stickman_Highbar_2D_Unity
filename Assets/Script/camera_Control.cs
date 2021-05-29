using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_Control : MonoBehaviour
{
    private Camera disp1;
    private GameObject hip;
    private float[] size_Def;

    // Start is called before the first frame update
    void Start()
    {
        disp1 = gameObject.GetComponent<Camera>();
        hip = GameObject.Find("Hip");

        size_Def = new float[] { getScreenBottomRight().x - 2, getScreenTopLeft().y - 2 };

        Debug.Log(getScreenTopLeft().x + ",    " + getScreenTopLeft().y);
        Debug.Log(getScreenBottomRight().x + ",    " + getScreenBottomRight().y);

        Debug.Log(disp1.orthographicSize);
    }

    // Update is called once per frame
    void Update()
    {

        if (hip.transform.position.y - disp1.transform.position.y > size_Def[1])
        {
            disp1.transform.Translate(0, hip.transform.position.y - disp1.transform.position.y - size_Def[1], 0);
        }
        else if (hip.transform.position.y - disp1.transform.position.y < -size_Def[1])
        {
            disp1.transform.Translate(0, hip.transform.position.y - disp1.transform.position.y + size_Def[1], 0);
        }
        else if (Mathf.Abs(disp1.transform.position.y) > 0.01f)
        {
            if ((hip.transform.position.y > -size_Def[1]) && (hip.transform.position.y < size_Def[1]))
            {
                disp1.transform.Translate(0, -0.01f * Mathf.Sign(disp1.transform.position.y), 0);
            }
        }

        if (hip.transform.position.x - disp1.transform.position.x > size_Def[0])
        {
            disp1.transform.Translate(hip.transform.position.x - disp1.transform.position.x - size_Def[0], 0, 0);
        }
        else if (hip.transform.position.x - disp1.transform.position.x < -size_Def[0])
        {
            disp1.transform.Translate(hip.transform.position.x - disp1.transform.position.x + size_Def[0], 0, 0);
        }
        else if (Mathf.Abs(disp1.transform.position.x) > 0.01f)
        {
            if ((hip.transform.position.x > -size_Def[0]) && (hip.transform.position.x < size_Def[0]))
            {
                disp1.transform.Translate(-0.01f * Mathf.Sign(disp1.transform.position.x), 0, 0);
            }
        }
    }

    private Vector3 getScreenTopLeft()
    {
        // ‰æ–Ê‚Ì¶ã‚ðŽæ“¾
        Vector3 topLeft = disp1.ScreenToWorldPoint(Vector3.zero);
        // ã‰º”½“]‚³‚¹‚é
        topLeft.Scale(new Vector3(1f, -1f, 1f));
        return topLeft;
    }

    private Vector3 getScreenBottomRight()
    {
        // ‰æ–Ê‚Ì‰E‰º‚ðŽæ“¾
        Vector3 bottomRight = disp1.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0.0f));
        // ã‰º”½“]‚³‚¹‚é
        bottomRight.Scale(new Vector3(1f, -1f, 1f));
        return bottomRight;
    }
}
