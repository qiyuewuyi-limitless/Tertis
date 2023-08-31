using Platform.TouchHandler;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TouchRegistrar : MonoBehaviour
{
    private Vector2 startPos;
    private Vector2 originalPos;
    public float determineDistance = 150f; //�����Ӧ����

    /** �����¼� */
    public static event Action<Vector2Int> TouchMove;
    public static event Action<int> TouchClick;
    public static event Action TouchDrop;

    private void Awake()
    {
        /** �����¼� */
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
                    if (swipeDirection.x > 0) // ���һ���
                    {
                        TouchMove.Invoke(Vector2Int.right);
                    }
                    else // ���󻬶�
                    {
                        TouchMove.Invoke(Vector2Int.left);
                    }
                }
                else
                {
                    if (swipeDirection.y > 0) // ���ϻ���
                    {
                        TouchDrop.Invoke();
                    }
                    else // ���»���
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