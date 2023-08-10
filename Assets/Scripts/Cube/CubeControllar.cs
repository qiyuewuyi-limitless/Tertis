using UnityEngine;
using System.Collections.Generic;

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

        /** x~z坐标系 */
        protected Vector3 left = Vector3.left;
        protected Vector3 right = Vector3.right;
        public Vector3 down = Vector3.back;

        private void Awake()
        {
            downTime = GameManager._instance.autoDownTime;
            downTimer = 0f;

            TouchRegistrar.TouchLeft += Left;
            TouchRegistrar.TouchRight += Right;
            TouchRegistrar.TouchDown += ExecuteDown;
            TouchRegistrar.TouchClick += Rotate;
            TouchRegistrar.TouchExchange += Exchange;

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
            if (user == null)
                return;

            user.transform.localPosition += direction;
            bool pass = GameManager._instance.CheckBoundary(user.transform);
            if (!pass)
                user.transform.localPosition -= direction;
            else
                ExecutePredict();

        }
        private void Left()
        {
            Move(left);
        }
        private void Right()
        {
            Move(right);
        }
        private void Down()
        {
            if (user == null)
                return;

            user.transform.localPosition += down;
            bool pass = GameManager._instance.CheckBoundary(user.transform);

            if (!pass)
            {
                Destroy(shadow);
                user.transform.localPosition -= down;
                GameManager._instance.ReachBoundary(user.transform);
            }
            else
                ExecutePredict();
        }
        private void Rotate()
        {
            if (user == null)
                return;

            user.transform.Rotate(transform.up, ROTATE_ANGLE, Space.World);
            bool pass = GameManager._instance.CheckBoundary(user.transform);
            if (!pass)
                user.transform.Rotate(transform.up, -ROTATE_ANGLE, Space.World);
            else
                ExecutePredict();

        }
        private void Exchange()
        {
            if (user == null)
                return;

            user.transform.localPosition = shadow.transform.localPosition;
            //影子到达边界，不需要额外判断
            GameManager._instance.ReachBoundary(user.transform);

        }
        private void ExecuteDown()
        {
            downTimer += downTime;
        }

        #endregion

        #region 预测
        private void PredictDown()
        {
            shadow.transform.localPosition += down;
            bool pass = GameManager._instance.CheckBoundary(shadow.transform);
            if (!pass)
                shadow.transform.localPosition -= down;
        }
        private Vector3 PredictShadow()
        {
            Vector3 cur = shadow.transform.localPosition;
            PredictDown();
            Vector3 pre = shadow.transform.localPosition;

            if (pre == cur)
                return pre;
            else
                return PredictShadow();
        }
        private void ExecutePredict()
        {
            shadow.transform.localPosition = user.transform.localPosition;
            shadow.transform.rotation = user.transform.rotation;
            PredictShadow();
        }
        #endregion


        public void SetUser(GameObject obj)
        {
            user = obj;
            shadow = Instantiate(user, user.transform.position, user.transform.rotation, transform);
            
            foreach (Transform child in shadow.transform)
            {
                child.GetComponent<Renderer>().material = transparentMaterial;
                if (child.childCount != 0)
                    Destroy(child.GetChild(0).gameObject);
            }


            ExecutePredict();
        }
        public void DeleteUser()
        {
            if (user)
                Destroy(user);
            if (shadow)
                Destroy(shadow);
            user = null;
            shadow = null;
        }
        public void SetTransparentMaterial(Material transparent)
        {
            transparentMaterial = transparent;
        }
        private void RemoveMoveEvent()
        {
            TouchRegistrar.TouchLeft -= Left;
            TouchRegistrar.TouchRight -= Right;
            TouchRegistrar.TouchDown -= ExecuteDown;
            TouchRegistrar.TouchClick -= Rotate;
            TouchRegistrar.TouchExchange -= Exchange;
        }

    }

}