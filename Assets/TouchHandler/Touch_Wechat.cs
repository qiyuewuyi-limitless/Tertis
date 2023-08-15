using System;
using System.Collections.Generic;
using UnityEngine;
using WeChatWASM;

namespace Platform.TouchHandler
{
    public struct WxTouch
    {
        public WeChatWASM.Touch touch;
        public TouchPhase phase;
    }
    public class Touch_Wechat : TouchHandler
    {
        List<WxTouch> wXTouches;

        Action<OnTouchStartListenerResult> actionStart;
        Action<OnTouchStartListenerResult> actionMove;
        Action<OnTouchStartListenerResult> actionEnd;
        bool inited = false;

        void Start()
        {
            if (inited)
            {
                return;
            }

            inited = true;
            wXTouches = new List<WxTouch>();

            actionStart = (touchevent) =>
            {
                foreach (WeChatWASM.Touch touch in touchevent.changedTouches)
                {
                    WxTouch wxTouch;
                    wxTouch.touch = touch;
                    wxTouch.phase = TouchPhase.Began;
                    wXTouches.Add(wxTouch);
                }
            };
            actionMove = (touchevent) =>
            {
                foreach (WeChatWASM.Touch touch in touchevent.changedTouches)
                {
                    WxTouch wxTouch;
                    wxTouch.touch = touch;
                    wxTouch.phase = TouchPhase.Moved;
                    wXTouches.Add(wxTouch);
                }
            };

            actionEnd = touchevent =>
            {
                foreach (WeChatWASM.Touch touch in touchevent.changedTouches)
                {
                    WxTouch wxTouch;
                    wxTouch.touch = touch;
                    wxTouch.phase = TouchPhase.Ended;
                    wXTouches.Add(wxTouch);
                }
            };

            WX.OnTouchStart(actionStart);
            WX.OnTouchEnd(actionEnd);
            WX.OnTouchMove(actionMove);

        }

        void Update()
        {
            if (wXTouches.Count > 0)
            {
                foreach (var touch in wXTouches)
                {
                    var position = new Vector2(touch.touch.clientX, touch.touch.clientY);
                    if (touch.phase == TouchPhase.Began)
                    {
                        CallTouchBegan(touch.touch.identifier, position);
                    }
                    else if (touch.phase == TouchPhase.Moved)
                    {
                        CallTouchMove(touch.touch.identifier, position);
                    }
                    else if (touch.phase == TouchPhase.Ended)
                    {
                        CallTouchEnd(touch.touch.identifier, position);
                    }
                    else if (touch.phase == TouchPhase.Canceled)
                    {
                        CallTouchEnd(touch.touch.identifier, position);
                    }
                }
                wXTouches.Clear();
            }
        }

        private void OnDestroy()
        {
            WX.OffTouchStart(actionStart);
            WX.OffTouchEnd(actionEnd);
            WX.OffTouchMove(actionMove);
        }
    }
}