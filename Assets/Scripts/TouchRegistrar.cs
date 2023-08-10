using Platform.TouchHandler;
using UnityEngine;
using UnityEngine.Events;

public class TouchRegistrar : MonoBehaviour
{
    private Vector2 startPos;
    private Vector2 originPos;
    public float determineDistance = 120f; //�����Ӧ����
    public float clickRange = 20f; //�������Χ
    public float exchangeDistance = 300f; //��С��������
    
    /** �����¼� */
    public static event UnityAction TouchLeft;
    public static event UnityAction TouchRight;
    public static event UnityAction TouchDown;
    public static event UnityAction TouchClick;
    public static event UnityAction TouchExchange;

    private void Awake()
    {
        /** �����¼� */
        TouchHandler.callTouchBegan += TouchBegan;
        TouchHandler.callTouchMove += TouchMove;
        TouchHandler.callTouchEnd += TouchEnd;

    }
    public void TouchBegan(long fingerId, Vector2 position)
    {
        originPos = startPos = position;
    }
    public void TouchMove(long fingerId, Vector2 position)
    {
        float swipDisAxisX = position.x - startPos.x;
        float swipDisAxisY = position.y - startPos.y;
        bool isX = Mathf.Abs(swipDisAxisX) > Mathf.Abs(swipDisAxisY) + 50f;

        if (Mathf.Abs(swipDisAxisX) > determineDistance && isX)
        {
            startPos.x = position.x;

            if (swipDisAxisX > 0) //�һ�
            {
                if (TouchRight != null)
                    TouchRight.Invoke();
            }
            else if (swipDisAxisX < 0) //��
            {
                if (TouchLeft != null)
                    TouchLeft.Invoke();
            }
        }
        else if (Mathf.Abs(swipDisAxisY) > determineDistance)
        {
            if(swipDisAxisY < 0)
            {
                startPos.y = position.y;
                if (TouchDown != null)
                    TouchDown.Invoke();
            }

        }
    }
    public void TouchEnd(long fingerId, Vector2 position)
    {
        float swipDisAxisX = position.x - originPos.x;
        float swipDisAxisY = position.y - originPos.y;

        if (Mathf.Abs(swipDisAxisX) <= clickRange && Mathf.Abs(swipDisAxisY) <= clickRange)
            if (TouchClick != null)
                TouchClick.Invoke();
        if(Mathf.Abs(swipDisAxisY) > exchangeDistance)
        {
            if (swipDisAxisY > 0)
                TouchExchange.Invoke();
        }
    }
}