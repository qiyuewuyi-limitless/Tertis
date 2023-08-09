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

        transform.localPosition += down;
        bool pass = GameManager._instance.CheckBoundary(transform);
        if (!pass)
        {
            transform.localPosition -= down;
            GameManager._instance.ReachBoundaryToStatic(gameObject);
        }

        GameManager._instance.UpdateIndex(transform);
    }
}
