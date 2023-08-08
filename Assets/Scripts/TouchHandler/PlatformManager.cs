using Platform.TouchHandler;
using UnityEngine;

namespace Platform
{
    public class PlatformManager : MonoBehaviour
    {
        public static PlatformManager _instance;

        //private AudioHandler _audioHandler;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        private void Start()
        {
            SetInitial();
            AddTouch();
            AddAudio();
        }

        #region 初始

        void SetInitial()
        {
            //TODO 平台
#if UNITY_EDITOR
            Application.targetFrameRate = 60;
#elif UNITY_ANDROID
            Application.targetFrameRate = 60;
#elif UNITY_IOS
            Application.targetFrameRate = 60;
#elif UNITY_WEBGL

#endif
        }


        #endregion 初始

        #region 触摸

        void AddTouch()
        {
            //TODO 平台
#if UNITY_EDITOR
            gameObject.AddComponent<Touch_Editor>();
#elif UNITY_ANDROID
            //gameObject.AddComponent<Touch_Mobile>();
            gameObject.AddComponent<Touch_Editor>();
#elif UNITY_IOS
            gameObject.AddComponent<Touch_Mobile>();
#elif UNITY_WEBGL
            gameObject.AddComponent<Touch_Wechat>();
#endif
        }

        #endregion 触摸

        #region 震动

        public void Vibrate(int vibrateType, float lastTime = 0)
        {
            //TODO 平台
#if UNITY_EDITOR
            
#elif UNITY_ANDROID
            // if (lastTime>0)
            // {
            //     Lofelt.NiceVibrations.LofeltHaptics.PlayMaximumAmplitudePattern(new[] {0f, lastTime});
            //     return;
            // }
            // switch (vibrateType)
            // {
            //     case 0:
            //         Lofelt.NiceVibrations.LofeltHaptics.PlayMaximumAmplitudePattern(new[] {0f, 0.03f});
            //         break;
            //     case 1:
            //         Lofelt.NiceVibrations.LofeltHaptics.PlayMaximumAmplitudePattern(new[] {0f, 0.1f});
            //         break;
            //     case 2:
            //         Lofelt.NiceVibrations.LofeltHaptics.PlayMaximumAmplitudePattern(new[] {0f, 0.2f});
            //         break;
            // }
#elif UNITY_IOS

#elif UNITY_WEBGL
            switch (vibrateType)
            {
                case 0:
                    WeChatWASM.WX.VibrateShort(new WeChatWASM.VibrateShortOption
                    {
                        type = "heavy"
                    });
                    break;
                case 1:
                    WeChatWASM.WX.VibrateLong(new WeChatWASM.VibrateLongOption());
                    break;
            }
#endif
        }

        #endregion 震动

        #region 音乐

        public void AddAudio()
        {
           // if (_audioHandler) return;

            GameObject audioObj = new GameObject();
            audioObj.transform.parent = transform;
            audioObj.name = "AudiManager";

           // _audioHandler = audioObj.AddComponent<AudioHandler>();
        }

        #endregion 音乐

    }
}
