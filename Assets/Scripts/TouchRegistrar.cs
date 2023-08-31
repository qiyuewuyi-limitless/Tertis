using Platform.TouchHandler;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TouchRegistrar : MonoBehaviour
{
    private Vector2 startPos;
    private Vector2 originalPos;
    public float determineDistance = 150f; //最短响应距离

    /** 发布事件 */
    public static event Action<Vector2Int> TouchMove;
    public static event Action<int> TouchClick;
    public static event Action TouchDrop;

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
        float swipeDistance = Vector2.Distance(startPos, position);
        Vector2 swipeDirection = position - startPos;
        if (swipeDistance > determineDistance)
        {
            if (swipeDistance > determineDistance)
            {
                if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
                {
                    if (swipeDirection.x > 0) // 向右滑动
                    {
                        TouchMove.Invoke(Vector2Int.right);
                    }
                    else // 向左滑动
                    {
                        TouchMove.Invoke(Vector2Int.left);
                    }
                }
                else
                {
                    if (swipeDirection.y > 0) // 向上滑动
                    {
                        TouchDrop.Invoke();
                    }
                    else // 向下滑动
                    {
                        TouchMove.Invoke(Vector2Int.down);
                    }
                }
            }
            startPos = position;
        }

    }
    public void TouchEnd(long fingerId, Vector2 position)
    {
        float swipeDistance = Vector2.Distance(originalPos, position);
        Vector2 swipeDirection = position - originalPos;
        if (swipeDistance < determineDistance)
        {
            TouchClick.Invoke(1);
        }
    }
}