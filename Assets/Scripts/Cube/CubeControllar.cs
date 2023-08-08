using UnityEngine;

namespace Assets.scripts
{
    public class CubeControllar : MonoBehaviour
    {
        const float ROTATE_ANGLE = 90f;
        private float downTime;
        private float downTimer;

        private GameObject user;
        private Material transparentMaterial;
        private GameObject shadow;

        private void Awake()
        {
            downTime = GameManager._instance.autoDownTime;
            downTimer = 0f;

            TouchRegistrar.TouchLeft += Left;
            TouchRegistrar.TouchRight += Right;
            TouchRegistrar.TouchDown += TouchDown;
            TouchRegistrar.TouchClick += TouchClick;
            TouchRegistrar.TouchExchange += TouchExchange;
        }
        private void FixedUpdate()
        {
            downTimer += Time.fixedDeltaTime;
            if (downTimer >= downTime)
            {
                downTimer = 0f;
                Down();
            }
            if (gameObject.transform.childCount == 0)
                Destroy(gameObject);
        }
        private void OnDestroy()
        {
            RemoveMoveEvent();
            Destroy(shadow);
        }

        #region 控制
        private void Move(Vector3 direction)
        {
            GameManager._instance.RefreshIndex(user.transform);

            user.transform.localPosition += direction;
            //user.transform.position += direction;
            bool pass = GameManager._instance.CheckBoundary(user.transform);

            if (!pass)
                user.transform.localPosition -= direction;
                //user.transform.position -= direction;
            else
                ExecutePredict();

            GameManager._instance.UpdateIndex(user.transform);

        }

        /** x轴 */
        private void Left()
        {
            Move(Vector3.left);
        }
        private void Right()
        {
            Move(Vector3.right);
        }
        /** z轴 */
        private void Down()
        {
            GameManager._instance.RefreshIndex(user.transform);

            user.transform.localPosition += Vector3.forward;
            //user.transform.localPosition += Vector3.back;
            //user.transform.position += Vector3.back;
            bool pass = GameManager._instance.CheckBoundary(user.transform);
            if (!pass)
            {
                user.transform.localPosition -= Vector3.forward;
                //user.transform.localPosition -= Vector3.back;
                //user.transform.position -= Vector3.back;
                Destroy(shadow);
                GameManager._instance.ReachBoundary(user.transform);
            }
            else
                ExecutePredict();

            GameManager._instance.UpdateIndex(user.transform);
        }
        private void TouchClick()
        {
            GameManager._instance.RefreshIndex(user.transform);

            user.transform.Rotate(transform.up, ROTATE_ANGLE, Space.World);
            bool pass = GameManager._instance.CheckBoundary(user.transform);

            if (!pass)
                user.transform.Rotate(transform.up, -ROTATE_ANGLE, Space.World);
                //user.transform.Rotate(transform.up, -ROTATE_ANGLE);
            else
                ExecutePredict();

            GameManager._instance.UpdateIndex(user.transform);
        }
        private void TouchExchange()
        {
            GameManager._instance.RefreshIndex(user.transform);
            user.transform.localPosition = shadow.transform.localPosition;
            //user.transform.position = shadow.transform.position;
            GameManager._instance.UpdateIndex(user.transform);
        }
        private void TouchDown()
        {
            downTimer += downTime;
        }

        #endregion

        #region 预测
        private void PredictDown()
        {
            shadow.transform.localPosition += Vector3.forward;
            //shadow.transform.localPosition += Vector3.back;
            //shadow.transform.position += Vector3.back;
            bool pass = GameManager._instance.CheckBoundary(shadow.transform);
            if (!pass)
                shadow.transform.localPosition -= Vector3.forward;
                //shadow.transform.localPosition -= Vector3.back;
                //shadow.transform.position -= Vector3.back;
        }
        private Vector3 PredictShadow()
        {
            Vector3 cur = shadow.transform.localPosition;
            //Vector3 cur = shadow.transform.position;
            PredictDown();
            Vector3 pre = shadow.transform.localPosition;
            //Vector3 pre = shadow.transform.position;
            if (pre == cur)
                return pre;
            else
                return PredictShadow();
        }
        private void ExecutePredict()
        {
            shadow.transform.localPosition = user.transform.localPosition;
            shadow.transform.rotation = user.transform.rotation;
            //shadow.transform.position = user.transform.position;
            //shadow.transform.rotation = user.transform.rotation;
            PredictShadow();
        }
        #endregion

        public void SetUser(GameObject obj)
        {
            if (user)
                Destroy(user);
            if (shadow)
                Destroy(shadow);
            
            user = obj;
            shadow = Instantiate(user, user.transform.position, user.transform.rotation, transform);
            foreach (Transform child in shadow.transform)
                child.GetComponent<Renderer>().material = transparentMaterial;
            ExecutePredict();
        }

        public void SetTransparentMaterial(Material transparent)
        {
            transparentMaterial = transparent;
        }
        private void RemoveMoveEvent()
        {
            TouchRegistrar.TouchLeft -= Left;
            TouchRegistrar.TouchRight -= Right;
            TouchRegistrar.TouchDown -= TouchDown;
            TouchRegistrar.TouchClick -= TouchClick;
            TouchRegistrar.TouchExchange -= TouchExchange;
        }
    }

}