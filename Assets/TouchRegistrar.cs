using Platform.TouchHandler;
using System;
using UnityEngine;
using UnityEngine.Events;

public class TouchRegistrar : MonoBehaviour
{
    private Vector2 startPos;
    private Vector2 originalPos;
    public float dropDistance = 300f;
    public float determineDistance = 200f; //最短响应距离
    public float clickRange = 20f; //最大点击范围

    /** 发布事件 */
    public static event Action<Vector2Int> TouchMove;
    public static event Action<int> TouchClick;
    public static event Action  TouchDrop;

    private void Awake()
    {
        /** 订阅事件 */
        TouchHandler.callTouchBegan += TouchBegan;
        TouchHandler.callTouchMove += Move;
        TouchHandler.callTouchEnd += TouchEnd;

    }
    public void TouchBegan(long fingerId, Vector2 position)
    {
        originalPos = startPos = position;
    }
    public void Move(long fingerId, Vector2 position)
    {
        float swipDisAxisX = position.x - startPos.x;
        float swipDisAxisY = position.y - startPos.y;
        bool isX = Mathf.Abs(swipDisAxisX) > Mathf.Abs(swipDisAxisY) + 50f;

        if (Mathf.Abs(swipDisAxisX) > determineDistance && isX)
        {
            startPos.x = position.x;

            if (swipDisAxisX > 0) //右滑
                TouchMove.Invoke(Vector2Int.right);
            else if (swipDisAxisX < 0) //左滑
                TouchMove.Invoke(Vector2Int.left);

        }
        else if (Mathf.Abs(swipDisAxisY) > determineDistance)
        {
            startPos.y = position.y;

            if (swipDisAxisY < 0) // 下滑
                TouchMove.Invoke(Vector2Int.down);
        }
    }
    public void TouchEnd(long fingerId, Vector2 position)
    {
        float swipDisAxisX = position.x - originalPos.x;
        float swipDisAxisY = position.y - originalPos.y;

        if (Mathf.Abs(swipDisAxisX) <= clickRange && Mathf.Abs(swipDisAxisY) <= clickRange)
            TouchClick.Invoke(1);
        if (swipDisAxisY > dropDistance) 
            TouchDrop.Invoke();
    }
                
}