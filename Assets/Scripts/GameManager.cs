using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.scripts
{
    public class GameManager : MonoBehaviour
    {
        const int width = 12; // 列宽
        const int height = 22; // 行高
        const int size = 1; // 单位尺寸
        const int maxHeight = 16; //允许的最大高度
        public float autoDownTime = 0.3f; //方块自动下落时间
        public Dictionary<string, GameObject> prefabAssests = new();
        public Material transparentMaterial;

        public static GameManager _instance;

        /** 方块 */
        private GameObject bigBox;
        private GameObject element;
        private GameObject next;
        private Vector3 curPoint;
        private Vector3 nextPoint;

        /** 控制器 */
        private CubeControllar elementControllar;
        private bool isNotHandle = true;
        /** 生成器 */
        public CubeGenerator elementGenerator;
        public RoleGenerator roleGenerator;

        /** 映射信息 */
        private Transform boundary; // 占位作用
        private Vector3 headPoint;
        private Dictionary<Vector3, string> pointDict = new(new Vector3EqualityComparer());
        private Transform[,] stateTable = new Transform[height, width];

        private void Awake()
        {
            /*            if (_instance == null)
                        {
                            _instance = this;
                            InitialGameMangerParameter();
                        }
                        else
                            Destroy(gameObject);*/
            _instance = this;
            InitialGameMangerParameter();
        }

        public bool CheckBoundary(Transform transform)
        {
            foreach (Transform child in transform)
            {
                GetInfo(child, out int row, out int column);
                if (stateTable[row, column])
                    return false; //检测到碰撞
            }
            return true;
        }
        public void ReachBoundary(Transform transform)
        {
            UpdateIndexToElement(transform);

            List<Transform> childs = new();
            foreach (Transform child in transform)
                childs.Add(child);
            foreach (Transform child in childs)
                child.parent = bigBox.transform;

            if (element.transform == transform)
            {
                elementControllar.DeleteUser();
                element = next;
                StartCoroutine(MoveCoroutine(delegate
                {
                    elementControllar.SetUser(element);
                    next = elementGenerator.GeneratorCube(nextPoint);
                }));
            } // 更换控制方块

            if (isNotHandle)
            {
                List<int> fullToRows = GetFullToRows();
                isNotHandle = false;
                StartCoroutine(HandleFullRow(fullToRows));
            }
        } // 频繁调用
        
        private bool isFullRow(int index)
        {
            for (int i = 1; i <= width - 2; i++)
                if (!stateTable[index, i])
                    return false;

            return true;
        }
        private List<int> GetFullToRows()
        {
            List<int> fullToRows = new();
            for (int i = 1; i <= height - 2; i++)
                if (isFullRow(i))
                    fullToRows.Add(i);
            fullToRows.Sort();
            fullToRows.Reverse();
            return fullToRows;
        }
        private IEnumerator HandleFullRow(List<int> fullToRows)
        {
            foreach (int i in fullToRows)
            {
                RoleMove(i);
                while (!RoleFinishMove(i)) yield return null;
                RemoveOneRow(i);
            }
            isNotHandle = true;
        } 
       
        private void RoleMove(int index)
        {
            GameObject role;
            for (int i = 1; i <= width - 2; i++)
                if (stateTable[index, i].childCount != 0)
                {
                    role = stateTable[index, i].GetChild(0).gameObject;
                    role.GetComponent<RoleControllar>().EnableAgent();
                }
        }
        private bool RoleFinishMove(int index)
        {
            for (int i = 1; i <= width - 2; i++)
                if (stateTable[index, i].childCount != 0)
                    return false;

            return true;
        }

        private Vector3 GetRealtivePosition(Transform transform)
        {
            Vector3 position = transform.localPosition;
            Transform currentTransform = transform.parent;
            position = currentTransform.localRotation * position;
            position += currentTransform.localPosition;

            return position;
        }
        private void GetInfo(Transform transform, out int row, out int column)
        {
            string info = pointDict[GetRealtivePosition(transform)];
            string[] infos = info.Split(" ");
            List<int> indexList = new List<int>();
            foreach (string rs in infos)
                indexList.Add(Convert.ToInt32(rs));

            row = indexList[0];
            column = indexList[1];
            return;
        }


        internal void UpdateIndexToElement(Transform transform)
        {
            foreach (Transform child in transform)
            {
                GetInfo(child, out int row, out int column);
                stateTable[row, column] = child.transform;
            }
        }
        private void RemoveOneRow(int index)
        {
            for (int i = 1; i <= width - 2; i++)
                Destroy(stateTable[index, i].gameObject);

            for (int i = index + 1; i <= height - 2; i++)
                for (int j = 1; j <= width - 2; j++)
                {
                    if(stateTable[i,j])
                        stateTable[i, j].localPosition += elementControllar.down;
                    
                    stateTable[i - 1, j] = stateTable[i, j];
                }
        }
        private void GolbalRefresh()
        {
            for (int i = 1; i <= height - 2; i++)
                for (int j = 1; j <= width - 2; j++)
                    if (stateTable[i, j])
                        Destroy(stateTable[i, j].gameObject);
        }

        #region 初始化
        private void InitialGameMangerParameter()
        {
            bigBox = GameObject.Find("BigBox");
            curPoint = GameObject.Find("ElementPoint").transform.localPosition;
            nextPoint = GameObject.Find("NextPoint").transform.localPosition;
            headPoint = transform.localPosition;

            boundary = new GameObject("Boundary").transform;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (i == 0 || j == 0 || j == width - 1 || i == height - 1)
                        stateTable[i, j] = boundary;
                    else
                        stateTable[i, j] = null;

                    Vector3 tmp = new(headPoint.x + size * j, headPoint.y, headPoint.z + size * i);
                    pointDict.Add(tmp, i + " " + j);
                }
            }
        }
        public void InitialGameMangerComponent()
        {

            elementGenerator = gameObject.AddComponent<CubeGenerator>();
            roleGenerator = gameObject.AddComponent<RoleGenerator>();
            elementControllar = gameObject.AddComponent<CubeControllar>();
            elementControllar.SetTransparentMaterial(transparentMaterial);

            element = elementGenerator.GeneratorCube(curPoint);
            next = elementGenerator.GeneratorCube(nextPoint);
            elementControllar.SetUser(element);

        }
        #endregion

        private IEnumerator MoveCoroutine(Action callback)
        {
            Vector3 cur = element.transform.localPosition;
            while (cur.z > curPoint.z)
            {
                element.transform.localPosition = new Vector3(cur.x, cur.y, cur.z - 4 * 0.1f);
                cur = element.transform.localPosition;
                yield return new WaitForSeconds(0.1f);
            }
            element.transform.localPosition = curPoint; // 消除误差
            callback();
        }
    }
}