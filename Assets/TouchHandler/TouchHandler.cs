using UnityEngine;

namespace Platform.TouchHandler
{
    public abstract class TouchHandler : MonoBehaviour
    {
        public delegate void TouchAction(long fingerId, Vector2 position);

        public static event TouchAction callTouchBegan;
        public static event TouchAction callTouchEnd;
        public static event TouchAction callTouchMove;

        public void CallTouchBegan(long fingerId, Vector2 position)
        {
            if (callTouchBegan != null) callTouchBegan(fingerId, position);
        }

        public void CallTouchMove(long fingerId, Vector2 position)
        {
            if (callTouchMove != null) callTouchMove(fingerId, position);
        }

        public void CallTouchEnd(long fingerId, Vector2 position)
        {
            if (callTouchEnd != null) callTouchEnd(fingerId, position);
        }
    }
}
