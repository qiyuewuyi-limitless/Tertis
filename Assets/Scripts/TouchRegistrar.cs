using Platform.TouchHandler;
using UnityEngine;
using UnityEngine.Events;

public class TouchRegistrar : MonoBehaviour
{
    private Vector2 startPos;

    public float determineDistance = 30f;
    public float exchangeDistance = 100f;
    
    /** 发布事件 */
    public static event UnityAction TouchLeft;
    public static event UnityAction TouchRight;
    public static event UnityAction TouchDown;
    public static event UnityAction TouchClick;
    public static event UnityAction TouchExchange;

    private void Awake()
    {
        /** 订阅事件 */
        TouchHandler.callTouchBegan += TouchBegan;
        TouchHandler.callTouchEnd += TouchEnd;
    }
    public void TouchBegan(long fingerId, Vector2 position)
    {
        startPos = position;
    }
    public void TouchEnd(long fingerId, Vector2 position)
    {
        float swipDisAxisX = position.x - startPos.x;
        float swipDisAxisY = position.y - startPos.y;
        bool isX = Mathf.Abs(swipDisAxisX) > Mathf.Abs(swipDisAxisY);
        if (Mathf.Abs(swipDisAxisX) > determineDistance && isX)
        {
            if (swipDisAxisX > 0) //右滑
            {
                if (TouchRight != null)
                    TouchRight.Invoke();
            }
            else if (swipDisAxisX < 0) //左滑
            {
                if (TouchLeft != null)
                    TouchLeft.Invoke();
            }
        }
        else if (Mathf.Abs(swipDisAxisY) > determineDistance)
        {
            if (Mathf.Abs(swipDisAxisY) > exchangeDistance)
                if (TouchExchange != null)
                    TouchExchange.Invoke();
                else
                if (TouchDown != null)
                    TouchDown.Invoke(); //下滑
        }
        else
            if (TouchClick != null)
            TouchClick.Invoke();
    }
}