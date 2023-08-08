using UnityEngine;

namespace Platform.TouchHandler
{
    public class Touch_Editor : TouchHandler
    {
        void Update()
        {
            //KeyInput();
            MouseInput();
        }

        private void MouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                CallTouchBegan(0, Input.mousePosition);
            }

            if (Input.GetMouseButton(0))
            {
                CallTouchMove(0, Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                CallTouchEnd(0, Input.mousePosition);
            }
        }

        
        Vector2 keyPosition;
        void KeyInput()
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                keyPosition = new Vector2(240, 150);
                CallTouchBegan(1, keyPosition);
            }

            if (Input.GetKey(KeyCode.D))
            {
                keyPosition.x = 340;
            }

            if (Input.GetKey(KeyCode.A))
            {
                keyPosition.x = 140;
            }

            if (Input.GetKey(KeyCode.W))
            {
                keyPosition.y = 250;
            }

            if (Input.GetKey(KeyCode.S))
            {
                keyPosition.y = 50;
            }

            if (Input.GetKeyUp(KeyCode.D))
            {
                keyPosition.x = 240;
                if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S))
                {
                    keyPosition = new Vector2(240, 150);
                    CallTouchEnd(1, keyPosition);
                }
            }

            if (Input.GetKeyUp(KeyCode.A))
            {
                keyPosition.x = 240;
                if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
                {
                    keyPosition = new Vector2(240, 150);
                    CallTouchEnd(1, keyPosition);
                }
            }

            if (Input.GetKeyUp(KeyCode.W))
            {
                keyPosition.y = 150;
                if (!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                {
                    keyPosition = new Vector2(240, 150);
                    CallTouchEnd(1, keyPosition);
                }
            }

            if (Input.GetKeyUp(KeyCode.S))
            {
                keyPosition.y = 150;
                if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                {
                    keyPosition = new Vector2(240, 150);
                    CallTouchEnd(1, keyPosition);
                }
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                CallTouchMove(1, keyPosition);
            }
        }
    }
}
