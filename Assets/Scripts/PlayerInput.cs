using UnityEngine;
using UnityEngine.Events;

public class PlayerInput : MonoBehaviour
{
    public static event UnityAction touchLeft;
    public static event UnityAction touchRight;
    public static event UnityAction touchDown;
    public static event UnityAction touchClick;

    private Vector2 touchStartPosition;
    private float minSwipeDistacne = 100f;
    void Update()
    {
        Touch();
        Input();
    }
    public void Input()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.A) && touchLeft != null)
            touchLeft.Invoke();
        else if (UnityEngine.Input.GetKeyDown(KeyCode.D) && touchRight != null)
            touchRight.Invoke();
        else if (UnityEngine.Input.GetKeyDown(KeyCode.Space) && touchClick != null)
            touchClick.Invoke();
        else if (UnityEngine.Input.GetKeyDown(KeyCode.S) && touchDown != null)
            touchDown.Invoke();
    }
    public void Touch()
    {
        if (UnityEngine.Input.touchCount > 0)
        {
            Debug.Log("´¥Ãþ²Ù×÷");
            Touch touch = UnityEngine.Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                float swipDisAxisX = touch.position.x - touchStartPosition.x;
                float swipDisAxisY = touch.position.y - touchStartPosition.y;
                if (Mathf.Abs(swipDisAxisX) >= minSwipeDistacne || Mathf.Abs(swipDisAxisY) >= minSwipeDistacne)
                {
                    if (Mathf.Abs(swipDisAxisX) >= Mathf.Abs(swipDisAxisY))
                    {
                        if (swipDisAxisX > 0) //ÓÒ»¬
                        {
                            touchRight.Invoke();
                        }
                        else //×ó»¬
                        {
                            touchLeft.Invoke();
                        }
                    }
                    else if (swipDisAxisY < 0)
                    {
                        touchDown.Invoke();
                    }
                    else { }
                }
                else // µã»÷
                    touchClick.Invoke();
            }
        }
    } 
}
