using Assets.scripts;
using UnityEngine;

public class StaticControllar : CubeControllar
{
    private float staticDownTime;
    private float staticDownTimer;

    private void Awake()
    {
        staticDownTime = GameManager._instance.autoDownTime;
        staticDownTimer = 0f;
    }

    private void FixedUpdate()
    {
        staticDownTimer += Time.fixedDeltaTime;
        if (staticDownTimer >= staticDownTime)
        {
            staticDownTimer = 0f;
            if (gameObject.transform.childCount != 0)
                Down();
        }
    }
    private void Down()
    {
        GameManager._instance.RefreshIndex(transform);

        transform.localPosition += Vector3.forward;
        //transform.localPosition += Vector3.back;
        //transform.position += Vector3.back;
        bool pass = GameManager._instance.CheckBoundary(transform);
        if (!pass)
        {
            transform.localPosition -= Vector3.forward;
            //transform.localPosition -= Vector3.back;
            //transform.position -= Vector3.back;
            GameManager._instance.ReachBoundary(transform);
        }

        GameManager._instance.UpdateIndex(transform);
    }
}
