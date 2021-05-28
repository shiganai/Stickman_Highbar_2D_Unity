using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class freefall : MonoBehaviour
{
    private GameObject human;
    private GameObject wrist;
    private GameObject body;
    private GameObject leg;
    private GameObject hip;

    public float m_Body = 1;
    private float l_Body;
    public float m_Leg = 1;
    private float l_Leg;
    public float g = 1;

    public float tau_Wrist = 0;
    public float tau_Hip = 0;

    private float x_Wrist;
    private float dx_Wrist = 0;
    private float y_Wrist;
    private float dy_Wrist = 0;

    private float th_Wrist;
    private float th_Hip;
    private float dth_Wrist = 0;
    private float dth_Hip = 0;

    private float l_Wrist_Bar = 0;

    public string status = "Onbar";

    private System.Diagnostics.Stopwatch sw;

    static class DLL
    {
        //[DllImport("find_dd_Onbar 1", CallingConvention = CallingConvention.StdCall)]
        //public static extern int add_function(float dth_Hip, float dth_Wrist, float g, float l_Body, float l_Leg, float l_Wrist_Bar, float m_Body, float m_Leg, float tau_Hip, float th_Hip, float th_Wrist);
    }

    // Start is called before the first frame update
    void Start()
    {

        sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        human = GameObject.Find("Human");
        wrist = GameObject.Find("Wrist");
        body = GameObject.Find("Body");
        leg = GameObject.Find("Leg");
        hip = GameObject.Find("Hip");

        th_Wrist = wrist.transform.localRotation.eulerAngles.z * Mathf.Deg2Rad;
        th_Hip = hip.transform.localRotation.eulerAngles.z * Mathf.Deg2Rad;

        x_Wrist = wrist.transform.position.x;
        y_Wrist = wrist.transform.position.y;

        l_Body = body.transform.localScale.y * 2f;
        l_Leg = leg.transform.localScale.y * 2f;

        sw.Stop();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(status.Equals("Onbar") || status.Equals("Inair"))
        {
            th_Wrist += dth_Wrist * Time.fixedDeltaTime;
            th_Hip += dth_Hip * Time.fixedDeltaTime;

            float[] dds;

            if (status.Equals("Onbar"))
            {
                dds = find_Dd_Onbar();
            }
            else if (status.Equals("Inair"))
            {
                dds = find_Dd_Inair();
                dx_Wrist += dds[2] * Time.fixedDeltaTime;
                dy_Wrist += dds[3] * Time.fixedDeltaTime;

                human.transform.Translate(dx_Wrist * Time.fixedDeltaTime, dy_Wrist * Time.fixedDeltaTime, 0);
            }
            else
            {
                dds = new float[] { 0, 0 };
            }

            dth_Wrist += dds[0] * Time.fixedDeltaTime;
            dth_Hip += dds[1] * Time.fixedDeltaTime;

            wrist.transform.Rotate(0, 0, dth_Wrist * Time.fixedDeltaTime * Mathf.Rad2Deg);
            hip.transform.Rotate(0, 0, dth_Hip * Time.fixedDeltaTime * Mathf.Rad2Deg);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("“–‚½‚Á‚½‚æ");
    }

    float[] find_Dd_Onbar()
    {
        float[] dds = new float[2];

        float t2 = Mathf.Cos(th_Hip);
        float t3 = Mathf.Sin(th_Hip);
        float t4 = Mathf.Sin(th_Wrist);
        float t5 = th_Hip + th_Wrist;
        float t6 = Mathf.Pow(dth_Hip, 2);
        float t7 = Mathf.Pow(dth_Wrist, 2);
        float t8 = Mathf.Pow(l_Body, 2);
        float t9 = Mathf.Pow(l_Leg, 2);
        float t10 = Mathf.Pow(l_Wrist_Bar, 2);
        float t11 = th_Hip * 2.0f;
        float t16 = 1.0f / l_Leg;
        float t20 = l_Body * l_Wrist_Bar * m_Body * 1.2e+1f;
        float t21 = l_Body * l_Wrist_Bar * m_Leg * 2.4e+1f;
        float t12 = Mathf.Cos(t11);
        float t13 = Mathf.Pow(t2, 2);
        float t14 = Mathf.Sin(t11);
        float t15 = Mathf.Sin(t5);
        float t17 = t5 + th_Hip;
        float t19 = m_Body * t8 * 4.0f;
        float t22 = m_Leg * t8 * 1.2e+1f;
        float t23 = m_Body * t10 * 1.2e+1f;
        float t24 = m_Leg * t10 * 1.2e+1f;
        float t25 = -t20;
        float t26 = -t21;
        float t18 = Mathf.Sin(t17);
        float ddth_Wrist = -(t16 * (l_Leg * tau_Hip * 2.4e+1f 
            + l_Body * t2 * tau_Hip * 3.6e+1f 
            - l_Wrist_Bar * t2 * tau_Hip * 3.6e+1f 
            - g * l_Body * l_Leg * m_Body * t4 * 1.2e+1f 
            - g * l_Body * l_Leg * m_Leg * t4 * 1.5e+1f 
            + g * l_Body * l_Leg * m_Leg * t18 * 9.0f 
            + g * l_Leg * l_Wrist_Bar * m_Body * t4 * 2.4e+1f 
            + g * l_Leg * l_Wrist_Bar * m_Leg * t4 * 1.5e+1f 
            - g * l_Leg * l_Wrist_Bar * m_Leg * t18 * 9.0f 
            - l_Body * m_Leg * t3 * t6 * t9 * 1.2e+1f 
            - l_Body * m_Leg * t3 * t7 * t9 * 1.2e+1f 
            - l_Leg * m_Leg * t7 * t8 * t14 * 9.0f 
            - l_Leg * m_Leg * t7 * t10 * t14 * 9.0f 
            + l_Wrist_Bar * m_Leg * t3 * t6 * t9 * 1.2e+1f 
            + l_Wrist_Bar * m_Leg * t3 * t7 * t9 * 1.2e+1f 
            - dth_Hip * dth_Wrist * l_Body * m_Leg * t3 * t9 * 2.4e+1f 
            + dth_Hip * dth_Wrist * l_Wrist_Bar * m_Leg * t3 * t9 * 2.4e+1f 
            + l_Body * l_Leg * l_Wrist_Bar * m_Leg * t7 * t14 * 1.8e+1f)) 
            / (m_Body * t8 * 8.0f 
            + m_Body * t10 * 2.4e+1f 
            + m_Leg * t8 * 1.5e+1f
            + m_Leg * t10 * 1.5e+1f 
            - l_Body * l_Wrist_Bar * m_Body * 2.4e+1f
            - l_Body * l_Wrist_Bar * m_Leg * 3.0e+1f
            - m_Leg * t8 * t12 * 9.0f
            - m_Leg * t10 * t12 * 9.0f
            + l_Body * l_Wrist_Bar * m_Leg * t12 * 1.8e+1f);

        //float a = (m_Body * t8 * 8.0f
        //    + m_Body * t10 * 2.4e+1f
        //    + m_Leg * t8 * 1.5e+1f
        //    + m_Leg * t10 * 1.5e+1f
        //    - l_Body * l_Wrist_Bar * m_Body * 2.4e+1f
        //    - l_Body * l_Wrist_Bar * m_Leg * 3.0e+1f
        //    - m_Leg * t8 * t12 * 9.0f
        //    - m_Leg * t10 * t12 * 9.0f
        //    + l_Body * l_Wrist_Bar * m_Leg * t12 * 1.8e+1f);

        float t27 = l_Body * l_Wrist_Bar * m_Leg * t13 * 1.8e+1f;
        float t28 = m_Leg * t8 * t13 * 9.0f;
        float t29 = m_Leg * t10 * t13 * 9.0f;
        float t30 = -t28;
        float t31 = -t29;
        float t32 = t19 + t22 + t23 + t24 + t25 + t26 + t27 + t30 + t31;
        float t33 = 1.0f / t32;
        float ddth_Hip = -t16 * t33
            * (l_Leg * 2.0f
            + l_Body * t2 * 3.0f
            - l_Wrist_Bar * t2 * 3.0f)
            * (g * l_Body * m_Body * t4 * 3.0f
            + g * l_Body * m_Leg * t4 * 6.0f
            + g * l_Leg * m_Leg * t15 * 3.0f
            - g * l_Wrist_Bar * m_Body * t4 * 6.0f
            - g * l_Wrist_Bar * m_Leg * t4 * 6.0f
            + l_Body * l_Leg * m_Leg * t3 * t6 * 3.0f
            - l_Leg * l_Wrist_Bar * m_Leg * t3 * t6 * 3.0f
            + dth_Hip * dth_Wrist * l_Body * l_Leg * m_Leg * t3 * 6.0f
            - dth_Hip * dth_Wrist * l_Leg * l_Wrist_Bar * m_Leg * t3 * 6.0f)
            + (t33 * (tau_Hip * 1.2e+1f
            + g * l_Leg * m_Leg * t15 * 6.0f
            - l_Body * l_Leg * m_Leg * t3 * t7 * 6.0f
            + l_Leg * l_Wrist_Bar * m_Leg * t3 * t7 * 6.0f)
            * (m_Body * t8
            + m_Body * t10 * 3.0f
            + m_Leg * t8 * 3.0f
            + m_Leg * t9
            + m_Leg * t10 * 3.0f
            - l_Body * l_Wrist_Bar * m_Body * 3.0f
            - l_Body * l_Wrist_Bar * m_Leg * 6.0f
            + l_Body * l_Leg * m_Leg * t2 * 3.0f
            - l_Leg * l_Wrist_Bar * m_Leg * t2 * 3.0f))
            / (m_Leg * t9);

        dds[0] = ddth_Wrist;
        dds[1] = ddth_Hip;

        return dds;
    }

    float[] find_Dd_Inair()
    {

        float t2 = Mathf.Cos(th_Hip);
        float t3 = Mathf.Cos(th_Wrist);
        float t4 = Mathf.Sin(th_Hip);
        float t5 = Mathf.Sin(th_Wrist);
        float t6 = th_Hip + th_Wrist;
        float t7 = Mathf.Pow(dth_Hip, 2f);
        float t8 = Mathf.Pow(dth_Wrist, 2f);
        float t9 = Mathf.Pow(l_Body, 2f);
        float t10 = Mathf.Pow(l_Body, 3f);
        float t11 = Mathf.Pow(l_Leg, 2f);
        float t12 = Mathf.Pow(l_Leg, 3f);
        float t13 = Mathf.Pow(m_Body, 2f);
        float t14 = Mathf.Pow(m_Body, 3f);
        float t15 = Mathf.Pow(m_Leg, 2f);
        float t16 = Mathf.Pow(m_Leg, 3f);
        float t17 = th_Hip * 2.0f;
        float t22 = 1.0f / l_Body;
        float t24 = 1.0f / l_Leg;
        float t25 = 1.0f / m_Body;
        float t26 = -th_Wrist;
        float t27 = m_Body * m_Leg * 2.5e+1f;
        float t18 = Mathf.Cos(t17);
        float t19 = Mathf.Sin(t17);
        float t20 = Mathf.Cos(t6);
        float t21 = Mathf.Sin(t6);
        float t23 = 1.0f / t9;
        float ddth_Wrist = (t23 * t24 * t25 * (l_Leg * t13 * tau_Hip * -4.8e+1f - l_Leg * t15 * tau_Hip * 1.2e+1f - l_Leg * m_Body * m_Leg * tau_Hip * 6.0e+1f - l_Body * t2 * t13 * tau_Hip * 3.6e+1f - l_Body * m_Body * m_Leg * t2 * tau_Hip * 3.6e+1f + l_Body * m_Body * t4 * t7 * t11 * t15 * 3.0f + l_Body * m_Body * t4 * t8 * t11 * t15 * 3.0f + l_Body * m_Leg * t4 * t7 * t11 * t13 * 1.2e+1f + l_Body * m_Leg * t4 * t8 * t11 * t13 * 1.2e+1f + l_Leg * m_Leg * t8 * t9 * t13 * t19 * (9.0f / 2.0f) + dth_Hip * dth_Wrist * l_Body * m_Body * t4 * t11 * t15 * 6.0f + dth_Hip * dth_Wrist * l_Body * m_Leg * t4 * t11 * t13 * 2.4e+1f)) / (t13 * 4.0f + t15 * 4.0f + m_Body * m_Leg * 1.7e+1f - m_Body * m_Leg * Mathf.Pow(t2, 2f) * 9.0f);
        float t28 = t6 + th_Hip;
        float t31 = t13 * 8.0f;
        float t32 = t15 * 8.0f;
        float t33 = t26 + th_Hip;
        float t29 = Mathf.Cos(t28);
        float t30 = Mathf.Sin(t28);
        float t34 = Mathf.Cos(t33);
        float t35 = Mathf.Sin(t33);
        float t36 = m_Body * m_Leg * t18 * 9.0f;
        float t37 = -t36;
        float t38 = t27 + t31 + t32 + t37;
        float t39 = 1.0f / t38;
        float ddth_Hip = (t23 * t25 * t39 * (t9 * t14 * tau_Hip * -1.2e+1f - t11 * t16 * tau_Hip * 1.2e+1f - m_Body * t9 * t15 * tau_Hip * 4.8e+1f - m_Body * t11 * t15 * tau_Hip * 6.0e+1f - m_Leg * t9 * t13 * tau_Hip * 6.0e+1f - m_Leg * t11 * t13 * tau_Hip * 4.8e+1f - l_Body * l_Leg * m_Body * t2 * t15 * tau_Hip * 7.2e+1f - l_Body * l_Leg * m_Leg * t2 * t13 * tau_Hip * 7.2e+1f + l_Body * m_Body * t4 * t7 * t12 * t16 * 3.0f + l_Body * m_Body * t4 * t8 * t12 * t16 * 3.0f + l_Leg * m_Leg * t4 * t8 * t10 * t14 * 3.0f + l_Body * t4 * t7 * t12 * t13 * t15 * 1.2e+1f + l_Body * t4 * t8 * t12 * t13 * t15 * 1.2e+1f + l_Leg * t4 * t8 * t10 * t13 * t15 * 1.2e+1f + t7 * t9 * t11 * t13 * t15 * t19 * (9.0f / 2.0f) + t8 * t9 * t11 * t13 * t15 * t19 * 9.0f + dth_Hip * dth_Wrist * l_Body * m_Body * t4 * t12 * t16 * 6.0f + dth_Hip * dth_Wrist * l_Body * t4 * t12 * t13 * t15 * 2.4e+1f + dth_Hip * dth_Wrist * t9 * t11 * t13 * t15 * t19 * 9.0f) * -2.0f) / (m_Leg * t11);
        float ddx = -t22 * t24 * t25 * t39 * (l_Body * t13 * t20 * tau_Hip * 6.0f + l_Body * t13 * t34 * tau_Hip * 1.8e+1f + l_Leg * t3 * t13 * tau_Hip * 4.8e+1f + l_Leg * t3 * t15 * tau_Hip * 2.4e+1f - l_Body * m_Body * m_Leg * t20 * tau_Hip * 1.2e+1f + l_Body * m_Body * m_Leg * t34 * tau_Hip * 3.6e+1f + l_Leg * m_Body * m_Leg * t3 * tau_Hip * 9.0e+1f - l_Leg * m_Body * m_Leg * t29 * tau_Hip * 1.8e+1f + l_Leg * t5 * t8 * t9 * t14 * 4.0f + l_Body * m_Body * t7 * t11 * t15 * t21 + l_Body * m_Body * t8 * t11 * t15 * t21 - l_Body * m_Body * t7 * t11 * t15 * t35 * 3.0f - l_Body * m_Body * t8 * t11 * t15 * t35 * 3.0f - l_Body * m_Leg * t7 * t11 * t13 * t21 * 2.0f - l_Body * m_Leg * t8 * t11 * t13 * t21 * 2.0f + l_Leg * m_Body * t5 * t8 * t9 * t32 - l_Body * m_Leg * t7 * t11 * t13 * t35 * 6.0f - l_Body * m_Leg * t8 * t11 * t13 * t35 * 6.0f + l_Leg * m_Leg * t5 * t8 * t9 * t13 * 1.5e+1f - l_Leg * m_Leg * t8 * t9 * t13 * t30 * 3.0f + dth_Hip * dth_Wrist * l_Body * m_Body * t11 * t15 * t21 * 2.0f - dth_Hip * dth_Wrist * l_Body * m_Body * t11 * t15 * t35 * 6.0f - dth_Hip * dth_Wrist * l_Body * m_Leg * t11 * t13 * t21 * 4.0f - dth_Hip * dth_Wrist * l_Body * m_Leg * t11 * t13 * t35 * 1.2e+1f);

        float ddy = t22 * t24 * t25 * t39 * (g * l_Body * l_Leg * t14 * -8.0f - l_Body * t13 * t21 * tau_Hip * 6.0f + l_Body * t13 * t35 * tau_Hip * 1.8e+1f - l_Leg * t5 * t13 * tau_Hip * 4.8e+1f - l_Leg * t5 * t15 * tau_Hip * 2.4e+1f - g * l_Body * l_Leg * m_Body * t15 * 8.0f - g * l_Body * l_Leg * m_Leg * t13 * 2.5e+1f + l_Body * m_Body * m_Leg * t21 * tau_Hip * 1.2e+1f + l_Body * m_Body * m_Leg * t35 * tau_Hip * 3.6e+1f - l_Leg * m_Body * m_Leg * t5 * tau_Hip * 9.0e+1f + l_Leg * m_Body * m_Leg * t30 * tau_Hip * 1.8e+1f + l_Leg * t3 * t8 * t9 * t14 * 4.0f + l_Body * m_Body * t7 * t11 * t15 * t20 + l_Body * m_Body * t8 * t11 * t15 * t20 + l_Body * m_Body * t7 * t11 * t15 * t34 * 3.0f + l_Body * m_Body * t8 * t11 * t15 * t34 * 3.0f - l_Body * m_Leg * t7 * t11 * t13 * t20 * 2.0f - l_Body * m_Leg * t8 * t11 * t13 * t20 * 2.0f + l_Leg * m_Body * t3 * t8 * t9 * t32 + l_Body * m_Leg * t7 * t11 * t13 * t34 * 6.0f + l_Body * m_Leg * t8 * t11 * t13 * t34 * 6.0f + l_Leg * m_Leg * t3 * t8 * t9 * t13 * 1.5e+1f - l_Leg * m_Leg * t8 * t9 * t13 * t29 * 3.0f + g * l_Body * l_Leg * m_Leg * t13 * t18 * 9.0f + dth_Hip * dth_Wrist * l_Body * m_Body * t11 * t15 * t20 * 2.0f + dth_Hip * dth_Wrist * l_Body * m_Body * t11 * t15 * t34 * 6.0f - dth_Hip * dth_Wrist * l_Body * m_Leg * t11 * t13 * t20 * 4.0f + dth_Hip * dth_Wrist * l_Body * m_Leg * t11 * t13 * t34 * 1.2e+1f);

        float[] dds = new float[] { ddth_Wrist, ddth_Hip, ddx, ddy };
        return dds;
    }

}
