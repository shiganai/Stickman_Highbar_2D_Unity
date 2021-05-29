using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class freefall : MonoBehaviour
{
    private GameObject human;
    private GameObject wrist;
    private GameObject body;
    private GameObject leg;
    private GameObject hip;

    private float m_Body = 30;
    private float l_Body;
    private float m_Leg = 30;
    private float l_Leg;
    public float g = 9.8f;
    private float myu = 1;

    //private float tau_Wrist = 0;
    public float tau_Hip = 0;
    public float tau_Hip_Self = 0;
    private float tau_Hip_C = 1e3f;
    public float tau_Hip_Max = 100;
    private float tau_Hip_Straighten_Power = 2e2f;

    private float[] th_Hip_Lim = new float[] { -90 * Mathf.Deg2Rad, 150 * Mathf.Deg2Rad };

    private float x_Wrist;
    private float dx_Wrist = 0;
    private float y_Wrist;
    private float dy_Wrist = 0;

    private float th_Wrist;
    private float th_Hip;
    private float dth_Wrist = 0;
    private float dth_Hip = 0;

    private float l_Wrist_Bar = 0f;
    private float dl_Wrist_Bar = 0;

    private string status_Onbar = "onbar";
    private string status_Inair = "inair";
    private string status_Catch = "catch";
    private string status;

    private Slider torque_Slider;
    private System.Diagnostics.Stopwatch sw;

    // Start is called before the first frame update
    void Start()
    {

        sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        torque_Slider = GameObject.Find("Torque_Slider").GetComponent<Slider>();

        human = GameObject.Find("Human");
        wrist = GameObject.Find("Wrist");
        body = GameObject.Find("Body");
        leg = GameObject.Find("Leg");
        hip = GameObject.Find("Hip");

        Initialized();

        th_Wrist = wrist.transform.localRotation.eulerAngles.z * Mathf.Deg2Rad;
        th_Hip = hip.transform.localRotation.eulerAngles.z * Mathf.Deg2Rad;

        x_Wrist = wrist.transform.position.x;
        y_Wrist = wrist.transform.position.y;

        l_Body = body.transform.localScale.y * 2f;
        l_Leg = leg.transform.localScale.y * 2f;

        //status = status_Onbar;
        //status = status_Catch;
        status = status_Inair;

        sw.Stop();
    }

    public void Initialized()
    {
        th_Wrist = 0;
        dth_Wrist = 0;

        th_Hip = 0;
        dth_Hip = 0;

        l_Wrist_Bar = 0;
        dl_Wrist_Bar = 0;

        x_Wrist = -l_Wrist_Bar * (-Mathf.Sin(th_Wrist));
        y_Wrist = -l_Wrist_Bar * Mathf.Cos(th_Wrist);

        status = status_Onbar;

        wrist.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, th_Wrist * Mathf.Rad2Deg));
        hip.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, th_Hip * Mathf.Rad2Deg));

        wrist.transform.localPosition = new Vector3(x_Wrist, y_Wrist, 0);

        torque_Slider.value = 0;

        //GameObject.Find("Main Camera").transform.position = new Vector3(0,0,-10);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        th_Wrist += dth_Wrist * Time.fixedDeltaTime;
        th_Hip += dth_Hip * Time.fixedDeltaTime;

        float[] dds;

        if (status.Equals(status_Onbar))
        {
            float ddth_Hip_Target = -2 * tau_Hip_Straighten_Power * dth_Hip - tau_Hip_Straighten_Power * th_Hip;
            float tau_Hip_To_Straighten = find_Tau_Hip_Onbar(ddth_Hip_Target);

            if (Mathf.Abs(tau_Hip_To_Straighten) > tau_Hip_Max)
            {
                tau_Hip_To_Straighten = Mathf.Sign(tau_Hip_To_Straighten) * tau_Hip_Max;
            }

            if (tau_Hip_Self > 0)
            {
                tau_Hip = (tau_Hip_Max - tau_Hip_To_Straighten) * tau_Hip_Self + tau_Hip_To_Straighten;
            }
            else
            {
                tau_Hip = (-tau_Hip_Max - tau_Hip_To_Straighten) * (-tau_Hip_Self) + tau_Hip_To_Straighten;
            }

            if (th_Hip < th_Hip_Lim[0] && dth_Hip < 0)
            {
                tau_Hip += -tau_Hip_C * dth_Hip;
            }
            else if (th_Hip > th_Hip_Lim[1] && dth_Hip > 0)
            {
                tau_Hip += -tau_Hip_C * dth_Hip;
            }

            dds = find_Dd_Onbar();

            dth_Wrist += dds[0] * Time.fixedDeltaTime;
            dth_Hip += dds[1] * Time.fixedDeltaTime;

            dx_Wrist = dth_Wrist * l_Wrist_Bar * (-Mathf.Cos(th_Wrist));
            dy_Wrist = dth_Wrist * l_Wrist_Bar * (-Mathf.Sin(th_Wrist));

            x_Wrist = -l_Wrist_Bar * (-Mathf.Sin(th_Wrist));
            y_Wrist = -l_Wrist_Bar * Mathf.Cos(th_Wrist);

            wrist.transform.Rotate(0, 0, dth_Wrist * Time.fixedDeltaTime * Mathf.Rad2Deg);
            hip.transform.Rotate(0, 0, dth_Hip * Time.fixedDeltaTime * Mathf.Rad2Deg);

            wrist.transform.position = new Vector3(x_Wrist, y_Wrist, 0);
        }
        else if (status.Equals(status_Inair))
        {
            float ddth_Hip_Target = -2 * tau_Hip_Straighten_Power * dth_Hip - tau_Hip_Straighten_Power * th_Hip;
            float tau_Hip_To_Straighten = find_Tau_Hip_Onbar(ddth_Hip_Target);

            if (Mathf.Abs(tau_Hip_To_Straighten) > tau_Hip_Max)
            {
                tau_Hip_To_Straighten = Mathf.Sign(tau_Hip_To_Straighten) * tau_Hip_Max;
            }

            if (tau_Hip_Self > 0)
            {
                tau_Hip = (tau_Hip_Max - tau_Hip_To_Straighten) * tau_Hip_Self + tau_Hip_To_Straighten;
            }
            else
            {
                tau_Hip = (-tau_Hip_Max - tau_Hip_To_Straighten) * (-tau_Hip_Self) + tau_Hip_To_Straighten;
            }

            if (th_Hip < th_Hip_Lim[0] && dth_Hip < 0)
            {
                tau_Hip += -tau_Hip_C * dth_Hip;
            }
            else if (th_Hip > th_Hip_Lim[1] && dth_Hip > 0)
            {
                tau_Hip += -tau_Hip_C * dth_Hip;
            }

            dds = find_Dd_Inair();
            dx_Wrist += dds[2] * Time.fixedDeltaTime;
            dy_Wrist += dds[3] * Time.fixedDeltaTime;

            dth_Wrist += dds[0] * Time.fixedDeltaTime;
            dth_Hip += dds[1] * Time.fixedDeltaTime;

            wrist.transform.Rotate(0, 0, dth_Wrist * Time.fixedDeltaTime * Mathf.Rad2Deg);
            hip.transform.Rotate(0, 0, dth_Hip * Time.fixedDeltaTime * Mathf.Rad2Deg);

            Vector3 moving_Vec = wrist.transform.InverseTransformDirection(dx_Wrist, dy_Wrist, 0) * Time.fixedDeltaTime;
            wrist.transform.Translate(moving_Vec);
        }
        else if (status.Equals(status_Catch))
        {
            tau_Hip = tau_Hip_Self * tau_Hip_Max;
            if (th_Hip < th_Hip_Lim[0] && dth_Hip < 0)
            {
                tau_Hip += -tau_Hip_C * dth_Hip;
            }
            else if (th_Hip > th_Hip_Lim[1] && dth_Hip > 0)
            {
                tau_Hip += -tau_Hip_C * dth_Hip;
            }

            dds = find_Dd_Catch();
            float f_Wrist_Bar = dds[3];
            if (Mathf.Sign(f_Wrist_Bar) == Mathf.Sign(dl_Wrist_Bar))
            {
                myu *= -1;
                dds = find_Dd_Catch();
            }

            float ddl_Wrist_Bar = dds[2];
            dl_Wrist_Bar += ddl_Wrist_Bar * Time.fixedDeltaTime;
            l_Wrist_Bar += dl_Wrist_Bar * Time.fixedDeltaTime;

            x_Wrist = -l_Wrist_Bar * (-Mathf.Sin(th_Wrist));
            y_Wrist = -l_Wrist_Bar * Mathf.Cos(th_Wrist);

            dx_Wrist = l_Wrist_Bar * dth_Wrist * Mathf.Cos(th_Wrist);
            dy_Wrist = -l_Wrist_Bar * dth_Wrist * (-Mathf.Sin(th_Wrist));

            dth_Wrist += dds[0] * Time.fixedDeltaTime;
            dth_Hip += dds[1] * Time.fixedDeltaTime;

            wrist.transform.Rotate(0, 0, dth_Wrist * Time.fixedDeltaTime * Mathf.Rad2Deg);
            hip.transform.Rotate(0, 0, dth_Hip * Time.fixedDeltaTime * Mathf.Rad2Deg);

            wrist.transform.position = new Vector3(x_Wrist, y_Wrist, 0);

            if(l_Wrist_Bar > l_Body)
            {
                status = "failed";
            }
        }
        else
        {
            dds = new float[] { 0, 0 };
        }
    }

    public void Set_Status_Onbar()
    {
        status = status_Onbar;
        float[] ds = find_Status_After_Slides_Collision();

        dth_Wrist = ds[0];
        dth_Hip = ds[1];

        dl_Wrist_Bar = 0;
    }
    public void Set_Status_Inair()
    {
        status = status_Inair;
    }
    public void Set_Status_Catch()
    {
        status = status_Catch;

        l_Wrist_Bar = wrist.transform.position.magnitude;
        dl_Wrist_Bar = -dx_Wrist * (-Mathf.Sin(th_Wrist)) - dy_Wrist * Mathf.Cos(th_Wrist);

        float l_N_Wrist_Bar = 0;
        float dl_N_Wrist_Bar = dx_Wrist * Mathf.Cos(th_Wrist) - dy_Wrist * (-Mathf.Sin(th_Wrist)) - l_Wrist_Bar * dth_Wrist;

        float[] ds_Tmp = find_Status_After_Catch(l_N_Wrist_Bar, dl_N_Wrist_Bar);
        float[] ds;
        float I_N_Wrist_Bar = ds_Tmp[3];
        if (Mathf.Sign(I_N_Wrist_Bar * myu) == Mathf.Sign(dl_Wrist_Bar))
        {
            myu *= -1;
            ds = find_Status_After_Catch(l_N_Wrist_Bar, dl_N_Wrist_Bar);
        }
        else
        {
            ds = ds_Tmp;
        }
        dth_Wrist = ds[0];
        dth_Hip = ds[1];
        dl_Wrist_Bar = ds[2];
    }

    public string Get_Status()
    {
        return status;
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

    float[] find_Dd_Catch()
    {

        float t2 = Mathf.Cos(th_Hip);
        float t3 = Mathf.Cos(th_Wrist);
        float t4 = Mathf.Sin(th_Hip);
        float t5 = Mathf.Sin(th_Wrist);
        float t6 = Mathf.Pow(dth_Hip, 2f);
        float t7 = Mathf.Pow(dth_Wrist, 2f);
        float t8 = Mathf.Pow(l_Body, 2f);
        float t9 = Mathf.Pow(l_Body, 3f);
        float t10 = Mathf.Pow(l_Leg, 2f);
        float t11 = Mathf.Pow(l_Leg, 3f);
        float t12 = Mathf.Pow(l_Wrist_Bar, 2f);
        float t13 = Mathf.Pow(l_Wrist_Bar, 3f);
        float t14 = Mathf.Pow(m_Body, 2f);
        float t15 = Mathf.Pow(m_Leg, 2f);
        float t16 = Mathf.Pow(m_Leg, 3f);
        float t17 = th_Hip * 2.0f;
        float t21 = 1.0f / l_Leg;
        float t22 = l_Leg * m_Leg * tau_Hip * 6.0f;
        float t23 = Mathf.PI / 2.0f;
        float t24 = l_Leg * m_Body * tau_Hip * 2.4e+1f;
        float t30 = l_Body * l_Wrist_Bar * m_Body * m_Leg * 9.0e+1f;
        float t36 = l_Body * l_Leg * m_Leg * myu * tau_Hip * 3.0e+1f;
        float t37 = l_Leg * l_Wrist_Bar * m_Body * myu * tau_Hip * 4.8e+1f;
        float t38 = l_Leg * l_Wrist_Bar * m_Leg * myu * tau_Hip * 3.0e+1f;
        float t51 = l_Body * l_Wrist_Bar * m_Body * m_Leg * tau_Hip * 2.16e+2f;
        float t52 = l_Body * l_Leg * m_Body * myu * tau_Hip * -2.4e+1f;
        float t54 = dl_Wrist_Bar * dth_Wrist * l_Body * l_Leg * m_Body * m_Leg * 5.4e+1f;
        float t55 = dl_Wrist_Bar * dth_Wrist * l_Leg * l_Wrist_Bar * m_Body * m_Leg * 6.0e+1f;
        float t18 = Mathf.Cos(t17);
        float t19 = Mathf.Pow(t2, 2f);
        float t20 = Mathf.Sin(t17);
        float t25 = t23 + th_Wrist;
        float t26 = l_Body * l_Wrist_Bar * t14 * 4.8e+1f;
        float t27 = l_Body * l_Wrist_Bar * t15 * 2.4e+1f;
        float t28 = m_Body * m_Leg * t8 * 4.0e+1f;
        float t29 = m_Body * m_Leg * t12 * 6.0e+1f;
        float t31 = l_Body * m_Body * t2 * tau_Hip * 3.6e+1f;
        float t32 = l_Body * m_Leg * t2 * tau_Hip * 3.6e+1f;
        float t33 = l_Wrist_Bar * m_Body * t2 * tau_Hip * 3.6e+1f;
        float t34 = l_Wrist_Bar * m_Leg * t2 * tau_Hip * 3.6e+1f;
        float t35 = l_Body * myu * t24;
        float t44 = -t30;
        float t47 = t10 * t15 * tau_Hip * 6.0f;
        float t48 = m_Body * m_Leg * t10 * tau_Hip * 2.4e+1f;
        float t53 = -t36;
        float t56 = t8 * t14 * 1.6e+1f;
        float t57 = t8 * t15 * 1.2e+1f;
        float t58 = t12 * t14 * 4.8e+1f;
        float t59 = t12 * t15 * 1.2e+1f;
        float t60 = l_Body * l_Wrist_Bar * m_Body * t4 * tau_Hip * 7.2e+1f;
        float t61 = l_Body * l_Wrist_Bar * m_Leg * t4 * tau_Hip * 1.44e+2f;
        float t62 = g * l_Body * l_Leg * t5 * t15 * 6.0f;
        float t63 = g * l_Leg * l_Wrist_Bar * t5 * t15 * 6.0f;
        float t65 = g * l_Body * l_Leg * m_Body * m_Leg * t5 * 2.7e+1f;
        float t66 = g * l_Leg * l_Wrist_Bar * m_Body * m_Leg * t5 * 3.0e+1f;
        float t67 = t8 * t14 * tau_Hip * 2.4e+1f;
        float t69 = l_Body * l_Wrist_Bar * t14 * tau_Hip * 7.2e+1f;
        float t70 = l_Body * l_Wrist_Bar * t15 * tau_Hip * 1.44e+2f;
        float t71 = m_Body * m_Leg * t8 * tau_Hip * 9.6e+1f;
        float t73 = m_Body * m_Leg * t12 * tau_Hip * 1.44e+2f;
        float t74 = dl_Wrist_Bar * dth_Wrist * l_Body * l_Leg * t14 * 2.4e+1f;
        float t75 = dl_Wrist_Bar * dth_Wrist * l_Body * l_Leg * t15 * 1.2e+1f;
        float t76 = dl_Wrist_Bar * dth_Wrist * l_Leg * l_Wrist_Bar * t14 * 4.8e+1f;
        float t77 = dl_Wrist_Bar * dth_Wrist * l_Leg * l_Wrist_Bar * t15 * 1.2e+1f;
        float t78 = -t54;
        float t80 = m_Body * t4 * t8 * tau_Hip * 2.4e+1f;
        float t84 = m_Body * myu * t2 * t8 * tau_Hip * 1.2e+1f;
        float t85 = g * l_Body * l_Leg * t5 * t14 * 1.2e+1f;
        float t87 = g * l_Leg * l_Wrist_Bar * t5 * t14 * 2.4e+1f;
        float t88 = l_Body * l_Leg * m_Body * m_Leg * t2 * tau_Hip * 7.2e+1f;
        float t89 = l_Leg * l_Wrist_Bar * m_Body * m_Leg * t2 * tau_Hip * 7.2e+1f;
        float t95 = t8 * t15 * tau_Hip * 7.2e+1f;
        float t96 = t12 * t14 * tau_Hip * 7.2e+1f;
        float t97 = t12 * t15 * tau_Hip * 7.2e+1f;
        float t102 = l_Body * l_Leg * m_Body * m_Leg * myu * t4 * tau_Hip * 1.8e+1f;
        float t103 = l_Leg * l_Wrist_Bar * m_Body * m_Leg * myu * t4 * tau_Hip * 3.6e+1f;
        float t104 = m_Leg * t4 * t8 * tau_Hip * 7.2e+1f;
        float t105 = m_Body * t4 * t12 * tau_Hip * 7.2e+1f;
        float t106 = m_Leg * t4 * t12 * tau_Hip * 7.2e+1f;
        float t107 = dl_Wrist_Bar * dth_Wrist * l_Body * t10 * t16 * 1.2e+1f;
        float t108 = dl_Wrist_Bar * dth_Wrist * l_Wrist_Bar * t10 * t16 * 1.2e+1f;
        float t109 = l_Leg * m_Body * m_Leg * t7 * t9 * 2.5e+1f;
        float t110 = l_Leg * m_Body * m_Leg * t7 * t13 * 6.0e+1f;
        float t112 = l_Body * l_Leg * t2 * t15 * tau_Hip * 7.2e+1f;
        float t113 = l_Leg * l_Wrist_Bar * t2 * t15 * tau_Hip * 7.2e+1f;
        float t122 = dl_Wrist_Bar * dth_Wrist * l_Leg * m_Body * m_Leg * myu * t8 * 2.0e+1f;
        float t127 = g * l_Body * t5 * t10 * t16 * 6.0f;
        float t128 = g * l_Wrist_Bar * t5 * t10 * t16 * 6.0f;
        float t130 = g * l_Leg * t3 * t30;
        float t134 = l_Body * l_Leg * myu * t4 * t15 * tau_Hip * 3.6e+1f;
        float t135 = l_Leg * l_Wrist_Bar * myu * t4 * t15 * tau_Hip * 3.6e+1f;
        float t138 = dth_Hip * dth_Wrist * l_Body * t4 * t10 * t15 * 6.0f;
        float t139 = dth_Hip * dth_Wrist * l_Body * t4 * t11 * t16 * 6.0f;
        float t140 = dth_Hip * dth_Wrist * l_Wrist_Bar * t4 * t10 * t15 * 6.0f;
        float t141 = dth_Hip * dth_Wrist * l_Wrist_Bar * t4 * t11 * t16 * 6.0f;
        float t142 = g * l_Leg * myu * t5 * t8 * t14 * 4.0f;
        float t143 = dth_Hip * dth_Wrist * l_Body * m_Body * m_Leg * t4 * t10 * 2.4e+1f;
        float t144 = dth_Hip * dth_Wrist * l_Wrist_Bar * m_Body * m_Leg * t4 * t10 * 2.4e+1f;
        float t145 = l_Leg * t7 * t9 * t14 * 8.0f;
        float t146 = l_Leg * t7 * t9 * t15 * 1.2e+1f;
        float t147 = l_Leg * t7 * t13 * t14 * 4.8e+1f;
        float t148 = l_Leg * t7 * t13 * t15 * 1.2e+1f;
        float t150 = g * l_Leg * m_Body * m_Leg * myu * t5 * t8 * 1.0e+1f;
        float t154 = dl_Wrist_Bar * dth_Wrist * l_Body * m_Leg * t10 * t14 * 2.4e+1f;
        float t155 = dl_Wrist_Bar * dth_Wrist * l_Body * m_Body * t10 * t15 * 5.4e+1f;
        float t156 = dl_Wrist_Bar * dth_Wrist * l_Wrist_Bar * m_Leg * t10 * t14 * 4.8e+1f;
        float t157 = dl_Wrist_Bar * dth_Wrist * l_Wrist_Bar * m_Body * t10 * t15 * 6.0e+1f;
        float t158 = dl_Wrist_Bar * dth_Wrist * l_Leg * myu * t8 * t14 * 8.0f;
        float t164 = g * l_Leg * m_Body * m_Leg * t3 * t8 * -4.0e+1f;
        float t165 = g * l_Leg * m_Body * m_Leg * t3 * t12 * -6.0e+1f;
        float t170 = l_Body * t4 * t6 * t10 * t15 * 3.0f;
        float t171 = l_Body * t4 * t6 * t11 * t16 * 3.0f;
        float t172 = l_Body * t4 * t7 * t10 * t15 * 3.0f;
        float t173 = l_Body * t4 * t7 * t11 * t16 * 3.0f;
        float t174 = l_Wrist_Bar * t4 * t6 * t10 * t15 * 3.0f;
        float t175 = l_Wrist_Bar * t4 * t6 * t11 * t16 * 3.0f;
        float t176 = l_Wrist_Bar * t4 * t7 * t10 * t15 * 3.0f;
        float t177 = l_Wrist_Bar * t4 * t7 * t11 * t16 * 3.0f;
        float t180 = l_Body * m_Body * m_Leg * t4 * t6 * t10 * 1.2e+1f;
        float t181 = l_Body * m_Body * m_Leg * t4 * t7 * t10 * 1.2e+1f;
        float t182 = l_Wrist_Bar * m_Body * m_Leg * t4 * t6 * t10 * 1.2e+1f;
        float t183 = l_Wrist_Bar * m_Body * m_Leg * t4 * t7 * t10 * 1.2e+1f;
        float t184 = g * l_Body * m_Leg * t5 * t10 * t14 * 1.2e+1f;
        float t185 = g * l_Body * m_Body * t5 * t10 * t15 * 2.7e+1f;
        float t186 = g * l_Wrist_Bar * m_Leg * t5 * t10 * t14 * 2.4e+1f;
        float t187 = g * l_Wrist_Bar * m_Body * t5 * t10 * t15 * 3.0e+1f;
        float t193 = dl_Wrist_Bar * dth_Wrist * l_Body * l_Leg * l_Wrist_Bar * m_Body * t2 * t15 * 3.6e+1f;
        float t194 = dl_Wrist_Bar * dth_Wrist * l_Body * l_Leg * l_Wrist_Bar * m_Leg * t2 * t14 * 3.6e+1f;
        float t195 = dth_Hip * dth_Wrist * l_Body * l_Wrist_Bar * m_Body * m_Leg * t2 * t10 * 4.8e+1f;
        float t199 = l_Leg * l_Wrist_Bar * t7 * t8 * t14 * 4.0e+1f;
        float t200 = l_Body * l_Leg * t7 * t12 * t15 * 3.6e+1f;
        float t201 = l_Leg * l_Wrist_Bar * t7 * t8 * t15 * 3.6e+1f;
        float t206 = l_Leg * l_Wrist_Bar * m_Body * m_Leg * t7 * t8 * 1.0e+2f;
        float t207 = l_Body * l_Leg * m_Body * m_Leg * t7 * t12 * 1.35e+2f;
        float t208 = g * l_Leg * t3 * t8 * t14 * -1.6e+1f;
        float t209 = g * l_Leg * t3 * t8 * t15 * -1.2e+1f;
        float t210 = g * l_Leg * t3 * t12 * t14 * -4.8e+1f;
        float t211 = g * l_Leg * t3 * t12 * t15 * -1.2e+1f;
        float t213 = g * l_Body * l_Leg * l_Wrist_Bar * m_Body * t2 * t5 * t15 * 1.8e+1f;
        float t214 = g * l_Body * l_Leg * l_Wrist_Bar * m_Leg * t2 * t5 * t14 * 1.8e+1f;
        float t226 = dth_Hip * dth_Wrist * m_Body * myu * t8 * t10 * t15 * 6.0f;
        float t230 = dl_Wrist_Bar * dth_Wrist * l_Leg * m_Leg * t2 * t8 * t14 * 1.2e+1f;
        float t231 = dth_Hip * dth_Wrist * m_Body * m_Leg * t2 * t8 * t10 * 1.6e+1f;
        float t232 = dth_Hip * dth_Wrist * m_Body * m_Leg * t2 * t10 * t12 * 4.8e+1f;
        float t233 = dth_Hip * dth_Wrist * l_Body * l_Wrist_Bar * m_Body * myu * t10 * t15 * 1.8e+1f;
        float t234 = l_Body * l_Wrist_Bar * m_Body * m_Leg * t2 * t6 * t10 * 2.4e+1f;
        float t235 = l_Body * l_Wrist_Bar * m_Body * m_Leg * t2 * t7 * t10 * 2.4e+1f;
        float t236 = l_Leg * m_Leg * t4 * t7 * t9 * t14 * 6.0f;
        float t241 = dth_Hip * dth_Wrist * l_Body * m_Body * t4 * t11 * t15 * 2.4e+1f;
        float t242 = dth_Hip * dth_Wrist * l_Wrist_Bar * m_Body * t4 * t11 * t15 * 2.4e+1f;
        float t243 = g * l_Body * l_Leg * m_Body * m_Leg * myu * t2 * t4 * t5 * 9.0f;
        float t246 = l_Body * l_Leg * t7 * t12 * t14 * 7.2e+1f;
        float t250 = dl_Wrist_Bar * dth_Wrist * l_Leg * m_Leg * myu * t4 * t8 * t14 * 6.0f;
        float t252 = g * l_Leg * m_Body * t2 * t5 * t8 * t15 * 6.0f;
        float t253 = g * l_Leg * m_Leg * t2 * t5 * t8 * t14 * 6.0f;
        float t258 = t2 * t6 * t8 * t10 * t15 * 6.0f;
        float t259 = t2 * t7 * t8 * t10 * t15 * 6.0f;
        float t260 = t2 * t6 * t10 * t12 * t15 * 6.0f;
        float t261 = t2 * t7 * t10 * t12 * t15 * 6.0f;
        float t262 = m_Body * myu * t6 * t8 * t10 * t15 * 3.0f;
        float t263 = m_Body * myu * t7 * t8 * t10 * t15 * 3.0f;
        float t269 = l_Body * l_Wrist_Bar * t2 * t6 * t10 * t15 * 1.2e+1f;
        float t270 = l_Body * l_Wrist_Bar * t2 * t7 * t10 * t15 * 1.2e+1f;
        float t272 = m_Body * m_Leg * t2 * t6 * t8 * t10 * 8.0f;
        float t273 = m_Body * m_Leg * t2 * t7 * t8 * t10 * 8.0f;
        float t274 = m_Body * m_Leg * t2 * t6 * t10 * t12 * 2.4e+1f;
        float t275 = m_Body * m_Leg * t2 * t7 * t10 * t12 * 2.4e+1f;
        float t276 = l_Body * l_Wrist_Bar * m_Body * myu * t6 * t10 * t15 * 9.0f;
        float t277 = l_Body * l_Wrist_Bar * m_Body * myu * t7 * t10 * t15 * 9.0f;
        float t278 = dth_Hip * dth_Wrist * l_Body * l_Wrist_Bar * t2 * t10 * t15 * -2.4e+1f;
        float t280 = g * l_Leg * m_Leg * myu * t4 * t5 * t8 * t14 * 3.0f;
        float t283 = l_Body * m_Body * t4 * t6 * t11 * t15 * 1.2e+1f;
        float t284 = l_Body * m_Body * t4 * t7 * t11 * t15 * 1.2e+1f;
        float t285 = l_Leg * m_Body * t4 * t7 * t9 * t15 * 1.8e+1f;
        float t286 = l_Wrist_Bar * m_Body * t4 * t6 * t11 * t15 * 1.2e+1f;
        float t287 = l_Wrist_Bar * m_Body * t4 * t7 * t11 * t15 * 1.2e+1f;
        float t293 = m_Body * m_Leg * myu * t4 * t6 * t8 * t10 * 4.0f;
        float t294 = m_Body * m_Leg * myu * t4 * t7 * t8 * t10 * 4.0f;
        float t295 = dl_Wrist_Bar * dth_Wrist * l_Leg * m_Body * myu * t4 * t8 * t15 * 2.4e+1f;
        float t296 = dth_Hip * dth_Wrist * m_Body * m_Leg * myu * t4 * t8 * t10 * 8.0f;
        float t300 = dth_Hip * dth_Wrist * l_Body * l_Wrist_Bar * m_Body * m_Leg * myu * t4 * t10 * -2.4e+1f;
        float t313 = l_Body * l_Leg * m_Body * t4 * t7 * t12 * t15 * 1.8e+1f;
        float t314 = l_Body * l_Leg * m_Leg * t4 * t7 * t12 * t14 * 1.8e+1f;
        float t315 = l_Leg * l_Wrist_Bar * m_Leg * t4 * t7 * t8 * t14 * 1.8e+1f;
        float t317 = l_Body * l_Wrist_Bar * m_Body * m_Leg * myu * t4 * t6 * t10 * -1.2e+1f;
        float t318 = l_Body * l_Wrist_Bar * m_Body * m_Leg * myu * t4 * t7 * t10 * -1.2e+1f;
        float t328 = l_Leg * l_Wrist_Bar * m_Body * t4 * t7 * t8 * t15 * -3.6e+1f;
        float t332 = g * l_Body * m_Body * myu * t2 * t4 * t5 * t10 * t15 * 9.0f;
        float t39 = Mathf.Cos(t25);
        float t40 = Mathf.Sin(t25);
        float t41 = t25 + th_Hip;
        float t42 = -t26;
        float t43 = -t27;
        float t49 = -t33;
        float t50 = -t34;
        float t64 = l_Wrist_Bar * myu * t31;
        float t68 = -t47;
        float t72 = -t48;
        float t79 = l_Leg * m_Leg * t19 * tau_Hip * 1.8e+1f;
        float t81 = -t60;
        float t82 = -t61;
        float t83 = l_Body * l_Wrist_Bar * m_Body * m_Leg * t18 * 1.8e+1f;
        float t86 = -t62;
        float t90 = l_Body * l_Leg * m_Leg * t20 * tau_Hip * 1.8e+1f;
        float t91 = l_Leg * l_Wrist_Bar * m_Leg * t20 * tau_Hip * 1.8e+1f;
        float t92 = -t65;
        float t93 = l_Leg * m_Leg * myu * t20 * tau_Hip * 9.0f;
        float t94 = -t67;
        float t98 = -t71;
        float t99 = -t73;
        float t100 = -t74;
        float t101 = -t75;
        float t111 = m_Body * m_Leg * t8 * t18 * 1.2e+1f;
        float t114 = -t84;
        float t115 = -t85;
        float t116 = -t88;
        float t119 = -t95;
        float t120 = -t96;
        float t121 = -t97;
        float t123 = g * l_Leg * t3 * t26;
        float t124 = g * l_Leg * t3 * t27;
        float t125 = g * l_Leg * t3 * t28;
        float t126 = g * l_Leg * t3 * t29;
        float t129 = m_Body * m_Leg * myu * t8 * t20 * 3.0f;
        float t131 = l_Body * l_Leg * m_Leg * myu * t18 * tau_Hip * 1.8e+1f;
        float t132 = l_Leg * l_Wrist_Bar * m_Leg * myu * t18 * tau_Hip * 1.8e+1f;
        float t133 = l_Body * l_Wrist_Bar * m_Body * m_Leg * myu * t20 * 9.0f;
        float t136 = -t103;
        float t137 = dl_Wrist_Bar * dth_Wrist * l_Body * l_Leg * m_Body * m_Leg * t19 * 1.8e+1f;
        float t149 = -t108;
        float t151 = -t110;
        float t153 = -t112;
        float t159 = g * l_Leg * t3 * t56;
        float t160 = g * l_Leg * t3 * t57;
        float t161 = g * l_Leg * t3 * t58;
        float t162 = g * l_Leg * t3 * t59;
        float t163 = -t122;
        float t166 = -t128;
        float t169 = -t135;
        float t178 = -t138;
        float t179 = -t141;
        float t188 = -t142;
        float t189 = -t143;
        float t190 = -t147;
        float t191 = -t148;
        float t192 = -t150;
        float t196 = dl_Wrist_Bar * dth_Wrist * l_Leg * m_Body * m_Leg * t8 * t20 * 6.0f;
        float t197 = dl_Wrist_Bar * dth_Wrist * l_Body * l_Leg * l_Wrist_Bar * m_Body * m_Leg * t20 * 1.8e+1f;
        float t198 = dl_Wrist_Bar * dth_Wrist * l_Body * l_Leg * m_Body * m_Leg * myu * t20 * 9.0f;
        float t202 = g * l_Body * l_Leg * m_Body * m_Leg * t5 * t19 * 9.0f;
        float t203 = -t156;
        float t204 = -t157;
        float t205 = -t158;
        float t212 = t10 * t15 * t19 * tau_Hip * 1.8e+1f;
        float t215 = g * l_Leg * m_Body * m_Leg * t5 * t8 * t20 * 3.0f;
        float t216 = -t170;
        float t217 = -t172;
        float t218 = -t175;
        float t219 = -t177;
        float t220 = g * l_Body * l_Leg * l_Wrist_Bar * m_Body * m_Leg * t5 * t20 * 9.0f;
        float t221 = -t180;
        float t222 = -t181;
        float t223 = -t186;
        float t224 = -t187;
        float t225 = myu * t10 * t15 * t20 * tau_Hip * 9.0f;
        float t227 = l_Leg * m_Body * m_Leg * t7 * t9 * t18 * 3.0f;
        float t228 = dth_Hip * dth_Wrist * t2 * t10 * t27;
        float t229 = dl_Wrist_Bar * dth_Wrist * l_Leg * m_Body * t2 * t57;
        float t237 = -t193;
        float t238 = -t194;
        float t239 = -t195;
        float t245 = -t199;
        float t247 = -t201;
        float t248 = -t206;
        float t254 = l_Wrist_Bar * myu * t143;
        float t255 = -t213;
        float t256 = -t214;
        float t264 = dth_Hip * dth_Wrist * t2 * t10 * t57;
        float t265 = dth_Hip * dth_Wrist * t2 * t10 * t59;
        float t267 = g * l_Leg * m_Body * m_Leg * myu * t5 * t8 * t18 * 6.0f;
        float t268 = -t226;
        float t281 = -t234;
        float t282 = -t235;
        float t288 = -t242;
        float t289 = -t243;
        float t292 = l_Leg * m_Body * m_Leg * myu * t7 * t9 * t20 * 3.0f;
        float t297 = g * l_Body * l_Leg * l_Wrist_Bar * m_Body * m_Leg * t3 * t18 * -1.8e+1f;
        float t298 = l_Wrist_Bar * myu * t180;
        float t299 = l_Wrist_Bar * myu * t181;
        float t301 = -t262;
        float t302 = -t263;
        float t303 = g * l_Leg * m_Body * m_Leg * myu * t3 * t8 * t20 * -3.0f;
        float t304 = -t269;
        float t305 = -t270;
        float t306 = g * l_Leg * m_Body * myu * t4 * t5 * t57;
        float t307 = -t286;
        float t308 = -t287;
        float t309 = l_Body * l_Leg * l_Wrist_Bar * m_Body * m_Leg * t7 * t20 * (9.0f / 2.0f);
        float t310 = dl_Wrist_Bar * dth_Wrist * l_Body * m_Body * t10 * t15 * t19 * 1.8e+1f;
        float t311 = l_Body * l_Leg * m_Body * m_Leg * t7 * t12 * t18 * 9.0f;
        float t316 = m_Body * t4 * t201;
        float t319 = dth_Hip * dth_Wrist * l_Body * l_Wrist_Bar * m_Body * t10 * t15 * t20 * 1.8e+1f;
        float t320 = dl_Wrist_Bar * dth_Wrist * l_Body * m_Body * myu * t10 * t15 * t20 * 9.0f;
        float t321 = l_Body * l_Leg * m_Body * m_Leg * myu * t7 * t12 * t20 * 9.0f;
        float t322 = l_Leg * l_Wrist_Bar * m_Body * m_Leg * myu * t7 * t8 * t20 * 1.2e+1f;
        float t323 = g * l_Body * m_Body * t5 * t10 * t15 * t19 * 9.0f;
        float t324 = l_Leg * m_Body * m_Leg * t7 * t8 * t20 * (9.0f / 2.0f);
        float t327 = -t315;
        float t329 = m_Body * t6 * t8 * t10 * t15 * t20 * 6.0f;
        float t330 = dth_Hip * dth_Wrist * m_Body * t10 * t20 * t57;
        float t331 = l_Body * l_Wrist_Bar * m_Body * t6 * t10 * t15 * t20 * 9.0f;
        float t337 = t19 * t226;
        float t338 = t19 * t233;
        float t340 = t19 * t262;
        float t341 = t19 * t263;
        float t342 = t19 * t276;
        float t343 = t19 * t277;
        float t344 = dth_Hip * dth_Wrist * l_Body * l_Wrist_Bar * m_Body * myu * t10 * t15 * t19 * -1.8e+1f;
        float t345 = l_Body * l_Wrist_Bar * m_Body * t7 * t10 * t15 * t20 * (2.7e+1f / 2.0f);
        float t346 = l_Body * l_Wrist_Bar * m_Body * myu * t6 * t10 * t15 * t19 * -9.0f;
        float t347 = l_Body * l_Wrist_Bar * m_Body * myu * t7 * t10 * t15 * t19 * -9.0f;
        float t348 = m_Body * t7 * t8 * t10 * t15 * t20 * (2.1e+1f / 2.0f);
        float t45 = Mathf.Cos(t41);
        float t46 = Mathf.Sin(t41);
        float t117 = -t91;
        float t118 = -t93;
        float t152 = -t111;
        float t167 = -t132;
        float t168 = -t133;
        float t240 = -t196;
        float t244 = -t198;
        float t249 = -t212;
        float t251 = g * l_Leg * t3 * t83;
        float t257 = -t215;
        float t266 = g * l_Leg * t3 * t129;
        float t271 = -t227;
        float t279 = g * l_Leg * t3 * t133;
        float t290 = dl_Wrist_Bar * dth_Wrist * l_Leg * myu * t111;
        float t291 = g * l_Leg * t3 * t111;
        float t312 = l_Leg * l_Wrist_Bar * t7 * t111;
        float t325 = -t310;
        float t326 = -t311;
        float t333 = -t319;
        float t334 = -t322;
        float t335 = -t323;
        float t336 = -t324;
        float t339 = -t331;
        float t349 = -t345;
        float t350 = t28 + t29 + t42 + t43 + t44 + t56 + t57 + t58 + t59 + t83 + t129 + t152 + t168;
        float t352 = t22 + t24 + t31 + t32 + t49 + t50 + t55 + t63 + t66 + t76 + t77 + t78 + t79 + t86 + t87 + t92 + t100 + t101 + t115 + t118 + t137 + t140 + t144 + t174 + t176 + t178 + t182 + t183 + t189 + t202 + t216 + t217 + t221 + t222 + t244 + t289 + t309 + t336;
        float t353 = t37 + t38 + t52 + t53 + t64 + t80 + t81 + t82 + t90 + t104 + t105 + t106 + t109 + t114 + t117 + t123 + t124 + t130 + t131 + t145 + t146 + t151 + t163 + t164 + t165 + t167 + t188 + t190 + t191 + t192 + t197 + t200 + t205 + t207 + t208 + t209 + t210 + t211 + t220 + t231 + t232 + t239 + t240 + t245 + t246 + t247 + t248 + t257 + t258 + t259 + t260 + t261 + t264 + t265 + t267 + t271 + t272 + t273 + t274 + t275 + t278 + t279 + t281 + t282 + t290 + t291 + t292 + t293 + t294 + t296 + t297 + t300 + t303 + t304 + t305 + t312 + t317 + t318 + t321 + t326 + t334;
        float t354 = t51 + t68 + t69 + t70 + t72 + t89 + t94 + t98 + t99 + t102 + t107 + t113 + t116 + t119 + t120 + t121 + t127 + t134 + t136 + t139 + t149 + t153 + t154 + t155 + t166 + t169 + t171 + t173 + t179 + t184 + t185 + t203 + t204 + t218 + t219 + t223 + t224 + t225 + t229 + t230 + t233 + t236 + t237 + t238 + t241 + t249 + t250 + t252 + t253 + t255 + t256 + t268 + t276 + t277 + t280 + t283 + t284 + t285 + t288 + t295 + t301 + t302 + t306 + t307 + t308 + t313 + t314 + t320 + t325 + t327 + t328 + t329 + t330 + t332 + t333 + t335 + t337 + t339 + t340 + t341 + t344 + t346 + t347 + t348 + t349;
        float t351 = 1.0f / t350;
        float ddth_Wrist = t21 * t351 * t352 * -2.0f;
        float ddth_Hip = (t351 * t354 * -2.0f) / (m_Leg * t10);
        float ddl_Wrist_Bar = -t21 * t351 * t353;
        float f_Wrist_Bar = myu * (g * m_Body * t5 + g * m_Leg * t5 + dl_Wrist_Bar * dth_Wrist * m_Body * t3 * t40 * 2.0f - dl_Wrist_Bar * dth_Wrist * m_Body * t5 * t39 * 2.0f + dl_Wrist_Bar * dth_Wrist * m_Leg * t3 * t40 * 2.0f - dl_Wrist_Bar * dth_Wrist * m_Leg * t5 * t39 * 2.0f - (l_Body * m_Body * t3 * t7 * t39) / 2.0f - (l_Body * m_Body * t5 * t7 * t40) / 2.0f - l_Body * m_Leg * t3 * t7 * t39 - l_Body * m_Leg * t5 * t7 * t40 - (l_Leg * m_Leg * t3 * t6 * t45) / 2.0f - (l_Leg * m_Leg * t3 * t7 * t45) / 2.0f - (l_Leg * m_Leg * t5 * t6 * t46) / 2.0f - (l_Leg * m_Leg * t5 * t7 * t46) / 2.0f + l_Wrist_Bar * m_Body * t3 * t7 * t39 + l_Wrist_Bar * m_Body * t5 * t7 * t40 + l_Wrist_Bar * m_Leg * t3 * t7 * t39 + l_Wrist_Bar * m_Leg * t5 * t7 * t40 + m_Leg * t3 * t46 * t351 * t352 - m_Leg * t5 * t45 * t351 * t352 + t3 * t21 * t46 * t351 * t354 - t5 * t21 * t45 * t351 * t354 + m_Body * t3 * t21 * t39 * t351 * t353 + m_Body * t5 * t21 * t40 * t351 * t353 + m_Leg * t3 * t21 * t39 * t351 * t353 + m_Leg * t5 * t21 * t40 * t351 * t353 - dth_Hip * dth_Wrist * l_Leg * m_Leg * t3 * t45 - dth_Hip * dth_Wrist * l_Leg * m_Leg * t5 * t46 + l_Body * m_Body * t3 * t21 * t40 * t351 * t352 - l_Body * m_Body * t5 * t21 * t39 * t351 * t352 + l_Body * m_Leg * t3 * t21 * t40 * t351 * t352 * 2.0f - l_Body * m_Leg * t5 * t21 * t39 * t351 * t352 * 2.0f - l_Wrist_Bar * m_Body * t3 * t21 * t40 * t351 * t352 * 2.0f + l_Wrist_Bar * m_Body * t5 * t21 * t39 * t351 * t352 * 2.0f - l_Wrist_Bar * m_Leg * t3 * t21 * t40 * t351 * t352 * 2.0f + l_Wrist_Bar * m_Leg * t5 * t21 * t39 * t351 * t352 * 2.0f);

        return new float[] { ddth_Wrist, ddth_Hip, ddl_Wrist_Bar, f_Wrist_Bar };
    }

    float[] find_Status_After_Catch(float l_N_Wrist_Bar, float dl_N_Wrist_Bar_Before)
    {
        float dth_Wrist_Before = dth_Wrist;
        float dth_Hip_Before = dth_Hip;
        float dl_Wrist_Bar_Before = dl_Wrist_Bar;

        float t2 = Mathf.Cos(th_Hip);
        float t3 = Mathf.Sin(th_Hip);
        float t4 = Mathf.Pow(l_Body, 2f);
        float t5 = Mathf.Pow(l_N_Wrist_Bar, 2f);
        float t6 = Mathf.Pow(l_Wrist_Bar, 2f);
        float t7 = Mathf.Pow(m_Body, 2f);
        float t8 = Mathf.Pow(m_Leg, 2f);
        float t9 = th_Hip * 2.0f;
        float t16 = l_Body * l_Wrist_Bar * m_Body * m_Leg * 9.0e+1f;
        float t17 = l_Body * l_N_Wrist_Bar * m_Body * m_Leg * myu * 4.5e+1f;
        float t18 = l_N_Wrist_Bar * l_Wrist_Bar * m_Body * m_Leg * myu * 6.0e+1f;
        float t10 = Mathf.Cos(t9);
        float t11 = Mathf.Sin(t9);
        float t12 = l_Body * l_Wrist_Bar * t7 * 4.8e+1f;
        float t13 = l_Body * l_Wrist_Bar * t8 * 2.4e+1f;
        float t14 = m_Body * m_Leg * t4 * 4.0e+1f;
        float t15 = m_Body * m_Leg * t6 * 6.0e+1f;
        float t21 = -t16;
        float t22 = l_Body * l_N_Wrist_Bar * myu * t7 * 2.4e+1f;
        float t23 = l_Body * l_N_Wrist_Bar * myu * t8 * 1.2e+1f;
        float t24 = l_N_Wrist_Bar * l_Wrist_Bar * myu * t7 * 4.8e+1f;
        float t25 = l_N_Wrist_Bar * l_Wrist_Bar * myu * t8 * 1.2e+1f;
        float t26 = -t18;
        float t27 = t4 * t7 * 1.6e+1f;
        float t28 = t4 * t8 * 1.2e+1f;
        float t29 = t6 * t7 * 4.8e+1f;
        float t30 = t6 * t8 * 1.2e+1f;
        float t19 = -t12;
        float t20 = -t13;
        float t31 = -t24;
        float t32 = -t25;
        float t33 = l_Body * l_Wrist_Bar * m_Body * m_Leg * t10 * 1.8e+1f;
        float t34 = m_Body * m_Leg * t4 * t10 * 1.2e+1f;
        float t35 = l_Body * l_N_Wrist_Bar * m_Body * m_Leg * myu * t10 * 9.0f;
        float t36 = m_Body * m_Leg * myu * t4 * t11 * 3.0f;
        float t37 = l_Body * l_Wrist_Bar * m_Body * m_Leg * myu * t11 * 9.0f;
        float t38 = -t34;
        float t39 = -t35;
        float t40 = -t37;
        float t41 = t14 + t15 + t17 + t19 + t20 + t21 + t22 + t23 + t26 + t27 + t28 + t29 + t30 + t31 + t32 + t33 + t36 + t38 + t39 + t40;
        float t42 = 1.0f / t41;
        float dth_Wrist_After = dth_Wrist_Before - dl_N_Wrist_Bar_Before * t42 * (l_Body * t7 * 8.0f + l_Body * t8 * 4.0f - l_Wrist_Bar * t7 * 1.6e+1f - l_Wrist_Bar * t8 * 4.0f + l_Body * m_Body * m_Leg * 1.5e+1f - l_Wrist_Bar * m_Body * m_Leg * 2.0e+1f + l_N_Wrist_Bar * myu * t7 * 1.6e+1f + l_N_Wrist_Bar * myu * t8 * 4.0f + l_N_Wrist_Bar * m_Body * m_Leg * myu * 2.0e+1f - l_Body * m_Body * m_Leg * t10 * 3.0f + l_Body * m_Body * m_Leg * myu * t11 * 3.0f) * 3.0f;
        float dth_Hip_After = dth_Hip_Before + (dl_N_Wrist_Bar_Before * t42 * (l_Body * l_Leg * t7 * 8.0f + l_Body * l_Leg * t8 * 4.0f - l_Leg * l_Wrist_Bar * t7 * 1.6e+1f - l_Leg * l_Wrist_Bar * t8 * 4.0f + t2 * t4 * t7 * 4.0f + l_Body * l_Leg * m_Body * m_Leg * 1.8e+1f - l_Leg * l_Wrist_Bar * m_Body * m_Leg * 2.0e+1f + l_Leg * l_N_Wrist_Bar * myu * t7 * 1.6e+1f + l_Leg * l_N_Wrist_Bar * myu * t8 * 4.0f - l_Body * l_Wrist_Bar * t2 * t7 * 1.2e+1f + m_Body * m_Leg * t2 * t4 * 4.0f + myu * t3 * t4 * t7 * 2.0f + l_Leg * l_N_Wrist_Bar * m_Body * m_Leg * myu * 2.0e+1f - l_Body * l_Wrist_Bar * m_Body * m_Leg * t2 * 1.2e+1f + l_Body * l_N_Wrist_Bar * myu * t2 * t7 * 1.2e+1f + m_Body * m_Leg * myu * t3 * t4 * 8.0f - l_Body * l_Leg * m_Body * m_Leg * Mathf.Pow(t2, 2f) * 6.0f + l_Body * l_Leg * m_Body * m_Leg * myu * t11 * 3.0f + l_Body * l_N_Wrist_Bar * m_Body * m_Leg * myu * t2 * 1.2e+1f) * 3.0f) / l_Leg;
        float dl_Wrist_Bar_After = dl_Wrist_Bar_Before - dl_N_Wrist_Bar_Before * t42 * (l_Body * l_N_Wrist_Bar * t7 * 2.4e+1f + l_Body * l_N_Wrist_Bar * t8 * 1.2e+1f - l_N_Wrist_Bar * l_Wrist_Bar * t7 * 4.8e+1f - l_N_Wrist_Bar * l_Wrist_Bar * t8 * 1.2e+1f + myu * t4 * t7 * 4.0f + myu * t5 * t7 * 4.8e+1f + myu * t5 * t8 * 1.2e+1f + l_Body * l_N_Wrist_Bar * m_Body * m_Leg * 4.5e+1f - l_N_Wrist_Bar * l_Wrist_Bar * m_Body * m_Leg * 6.0e+1f + m_Body * m_Leg * myu * t4 * 1.0e+1f + m_Body * m_Leg * myu * t5 * 6.0e+1f + m_Body * m_Leg * t4 * t11 * 3.0f - l_Body * l_N_Wrist_Bar * m_Body * m_Leg * t10 * 9.0f - l_Body * l_Wrist_Bar * m_Body * m_Leg * t11 * 9.0f - m_Body * m_Leg * myu * t4 * t10 * 6.0f + l_Body * l_N_Wrist_Bar * m_Body * m_Leg * myu * t11 * 1.8e+1f);
        float I_N_Wrist_Bar = dl_N_Wrist_Bar_Before * m_Body * t4 * t42 * (t7 * 8.0f + t8 * 8.0f + m_Body * m_Leg * 2.5e+1f - m_Body * m_Leg * t10 * 9.0f) * (-1.0f / 2.0f);

        return new float[] { dth_Wrist_After, dth_Hip_After, dl_Wrist_Bar_After, I_N_Wrist_Bar };
    }

    float[] find_Status_After_Slides_Collision()
    {

        float dth_Wrist_Before = dth_Wrist;
        float dth_Hip_Before = dth_Hip;
        float dl_Wrist_Bar_Before = dl_Wrist_Bar;

        float t2 = Mathf.Cos(th_Hip);
        float t3 = Mathf.Sin(th_Hip);
        float t4 = Mathf.Pow(l_Body, 2f);
        float t5 = Mathf.Pow(l_Wrist_Bar, 2f);
        float t6 = Mathf.Pow(m_Body, 2f);
        float t7 = Mathf.Pow(m_Leg, 2f);
        float t8 = th_Hip * 2.0f;
        float t12 = l_Body * l_Wrist_Bar * m_Body * 2.4e+1f;
        float t13 = l_Body * l_Wrist_Bar * m_Leg * 3.0e+1f;
        float t9 = Mathf.Cos(t8);
        float t10 = Mathf.Pow(t2, 2f);
        float t11 = Mathf.Sin(t8);
        float t14 = m_Body * t4 * 8.0f;
        float t15 = m_Leg * t4 * 1.5e+1f;
        float t16 = m_Body * t5 * 2.4e+1f;
        float t17 = m_Leg * t5 * 1.5e+1f;
        float t18 = -t12;
        float t19 = -t13;
        float t20 = l_Body * l_Wrist_Bar * m_Leg * t9 * 1.8e+1f;
        float t21 = m_Leg * t4 * t9 * 9.0f;
        float t22 = m_Leg * t5 * t9 * 9.0f;
        float t23 = -t21;
        float t24 = -t22;
        float t25 = t14 + t15 + t16 + t17 + t18 + t19 + t20 + t23 + t24;
        float t26 = 1.0f / t25;
        float dth_Wrist_After = t26 * (dth_Wrist_Before * t14 + dth_Wrist_Before * t15 + dth_Wrist_Before * t16 + dth_Wrist_Before * t17 + dth_Wrist_Before * t20 - dth_Wrist_Before * l_Body * l_Wrist_Bar * m_Body * 2.4e+1f - dth_Wrist_Before * l_Body * l_Wrist_Bar * m_Leg * 3.0e+1f - dl_Wrist_Bar_Before * l_Body * m_Leg * t11 * 9.0f + dl_Wrist_Bar_Before * l_Wrist_Bar * m_Leg * t11 * 9.0f - dth_Wrist_Before * m_Leg * t4 * t9 * 9.0f - dth_Wrist_Before * m_Leg * t5 * t9 * 9.0f);
        float dth_Hip_After = (t26 * (dth_Hip_Before * l_Leg * t14 + dth_Hip_Before * l_Leg * t15 + dth_Hip_Before * l_Leg * t16 + dth_Hip_Before * l_Leg * t17 + dth_Hip_Before * l_Leg * t20 + dl_Wrist_Bar_Before * m_Body * t3 * t4 * 1.2e+1f + dl_Wrist_Bar_Before * m_Body * t3 * t5 * 3.6e+1f + dl_Wrist_Bar_Before * m_Leg * t3 * t4 * 3.6e+1f + dl_Wrist_Bar_Before * m_Leg * t3 * t5 * 3.6e+1f - dth_Hip_Before * l_Body * l_Leg * l_Wrist_Bar * m_Body * 2.4e+1f - dth_Hip_Before * l_Body * l_Leg * l_Wrist_Bar * m_Leg * 3.0e+1f + dl_Wrist_Bar_Before * l_Body * l_Leg * m_Leg * t11 * 9.0f - dl_Wrist_Bar_Before * l_Body * l_Wrist_Bar * m_Body * t3 * 3.6e+1f - dl_Wrist_Bar_Before * l_Body * l_Wrist_Bar * m_Leg * t3 * 7.2e+1f - dl_Wrist_Bar_Before * l_Leg * l_Wrist_Bar * m_Leg * t11 * 9.0f - dth_Hip_Before * l_Leg * m_Leg * t4 * t9 * 9.0f - dth_Hip_Before * l_Leg * m_Leg * t5 * t9 * 9.0f)) / l_Leg;
        //float I_F_Wrist_Bar = -(dl_Wrist_Bar_Before * (m_Body * t17 + t4 * t6 * 4.0f + t4 * t7 * 3.0f + t5 * t6 * 1.2e+1f + t5 * t7 * 3.0f - l_Body * l_Wrist_Bar * t6 * 1.2e+1f - l_Body * l_Wrist_Bar * t7 * 6.0f + m_Body * m_Leg * t4 * 1.3e+1f - l_Body * l_Wrist_Bar * m_Body * m_Leg * 2.7e+1f - m_Body * m_Leg * t4 * t10 * 6.0f + l_Body * l_Wrist_Bar * m_Body * m_Leg * t10 * 9.0f)) / (m_Body * t4 * 4.0f + m_Body * t5 * 1.2e+1f + m_Leg * t4 * 1.2e+1f + m_Leg * t5 * 1.2e+1f - l_Body * l_Wrist_Bar * m_Body * 1.2e+1f - l_Body * l_Wrist_Bar * m_Leg * 2.4e+1f - m_Leg * t4 * t10 * 9.0f - m_Leg * t5 * t10 * 9.0f + l_Body * l_Wrist_Bar * m_Leg * t10 * 1.8e+1f);

        return new float[] { dth_Wrist_After, dth_Hip_After };
    }

    float find_Tau_Hip_Onbar(float ddth_Hip)
    {
        float t2 = Mathf.Cos(th_Hip);
        float t3 = Mathf.Sin(th_Hip);
        float t4 = Mathf.Sin(th_Wrist);
        float t5 = th_Hip + th_Wrist;
        float t6 = Mathf.Pow(dth_Hip, 2f);
        float t7 = Mathf.Pow(dth_Wrist, 2f);
        float t8 = Mathf.Pow(l_Body, 2f);
        float t9 = Mathf.Pow(l_Leg, 2f);
        float t10 = Mathf.Pow(l_Wrist_Bar, 2f);
        float t11 = Mathf.Sin(t5);
        float tau_Hip_Onbar = (l_Leg * m_Leg * (ddth_Hip * l_Leg * 4.0f - g * t11 * 6.0f + l_Body * t3 * t7 * 6.0f - l_Wrist_Bar * t3 * t7 * 3.0f)) / 1.2e+1f - (l_Leg * l_Wrist_Bar * m_Leg * t3 * t7) / 4.0f - (l_Leg * m_Leg * (l_Leg * 2.0f + l_Body * t2 * 3.0f - l_Wrist_Bar * t2 * 3.0f) * (ddth_Hip * m_Leg * t9 * 2.0f - g * l_Body * m_Body * t4 * 3.0f - g * l_Body * m_Leg * t4 * 6.0f - g * l_Leg * m_Leg * t11 * 3.0f + g * l_Wrist_Bar * m_Body * t4 * 6.0f + g * l_Wrist_Bar * m_Leg * t4 * 6.0f + ddth_Hip * l_Body * l_Leg * m_Leg * t2 * 3.0f - ddth_Hip * l_Leg * l_Wrist_Bar * m_Leg * t2 * 3.0f - l_Body * l_Leg * m_Leg * t3 * t6 * 3.0f + l_Leg * l_Wrist_Bar * m_Leg * t3 * t6 * 3.0f - dth_Hip * dth_Wrist * l_Body * l_Leg * m_Leg * t3 * 6.0f + dth_Hip * dth_Wrist * l_Leg * l_Wrist_Bar * m_Leg * t3 * 6.0f)) / (m_Body * t8 * 1.2e+1f + m_Body * t10 * 3.6e+1f + m_Leg * t8 * 3.6e+1f + m_Leg * t9 * 1.2e+1f + m_Leg * t10 * 3.6e+1f - l_Body * l_Wrist_Bar * m_Body * 3.6e+1f - l_Body * l_Wrist_Bar * m_Leg * 7.2e+1f + l_Body * l_Leg * m_Leg * t2 * 3.6e+1f - l_Leg * l_Wrist_Bar * m_Leg * t2 * 3.6e+1f);

        return tau_Hip_Onbar;
    }
}
