using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.scripts
{
    public class GameManager : MonoBehaviour
    {
        const int width = 12; // 列宽
        const int height = 22; // 行高
        const int size = 1; // 单位尺寸
        const int maxHeight = 4; //允许的最大高度
        public float autoDownTime = 0.3f; //方块自动下落时间
        public Transform offestTransform; //旋转偏移量
        private Transform boundary; // 占位作用
        public Dictionary<string, GameObject> prefabAssests = new();
        public Material transparentMaterial;

        public static GameManager _instance;

        /** 方块 */
        private GameObject item;
        public CubeControllar itemControllar;
        public List<GameObject> rows = new();

        /** 生成器 */
        public CubeGenerator itemGenerator;
        public RoleGenerator roleGenerator;

        /** 映射信息 */
        private Vector3 headPoint;
        private Dictionary<Vector3, string> pointDict = new(new Vector3EqualityComparer());
        public Transform[,] stateTable = new Transform[height, width];

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                InitialParameter();
            }
            else
                Destroy(gameObject);
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
            if (this.rows[maxHeight].transform.childCount != 0)
            {
                CleanTable();
                item = itemGenerator.GeneratorCube();
                itemControllar.SetUser(item);
                return;
            }

            List<int> rows = new();
            List<Transform> childs = new();
            foreach (Transform child in transform)
            {
                GetInfo(child, out int row, out int column);
                stateTable[row, column] = child.transform;
                rows.Add(row);
                childs.Add(child);
            }
            for (int i = 0; i < rows.Count; i++)
                childs[i].parent = this.rows[rows[i]].transform;

            if (item.transform == transform)
            {
                item = itemGenerator.GeneratorCube();
                itemControllar.SetUser(item);
            }
            FindFullRows();
        }
        private void FindFullRows()
        {
            foreach (GameObject control in rows)
                if (control.transform.childCount == width - 2)
                    RoleManager._instance.HandleFullRow(control);
        }
        public void RemoveCubs(Transform ancestor)
        {
            foreach (Transform child in ancestor)
                Destroy(child.gameObject);
        }

        #region 表更新
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
            //string info = pointDict[transform.position];
            string info = pointDict[GetRealtivePosition(transform)];
            string[] infos = info.Split(" ");
            List<int> indexList = new List<int>();
            foreach (string rs in infos)
                indexList.Add(Convert.ToInt32(rs));

            row = indexList[0];
            column = indexList[1];
            return;
        }
        internal void UpdateIndex(Transform transform)
        {
            foreach (Transform child in transform)
            {
                GetInfo(child, out int row, out int column);
                stateTable[row, column] = child.transform;
            }
        }
        internal void RefreshIndex(Transform transform)
        {
            foreach (Transform child in transform)
            {
                GetInfo(child, out int row, out int column);
                stateTable[row, column] = null;
            }
        }
        private void CleanTable()
        {
            for (int i = 1; i < height - 1; i++)
                for (int j = 1; j < width - 1; j++)
                {
                    if (stateTable[i, j] != null)
                        Destroy(stateTable[i, j].gameObject);

                    stateTable[i, j] = null;
                }
        }
        #endregion

        private void InitialParameter()
        {
            offestTransform = transform;
            //headPoint = transform.position;
            headPoint = transform.localPosition;

            for (int i = 0; i < height - 1; i++)
            {
                GameObject row = new("row" + i);
                row.transform.parent = transform;
                row.transform.localPosition = Vector3.zero;
                row.transform.localRotation = Quaternion.identity;
                row.AddComponent<StaticControllar>();
                rows.Add(row);
            }

            boundary = rows[0].transform;
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
        public void InitialGameManger()
        {
            
            itemGenerator = gameObject.AddComponent<CubeGenerator>();
            roleGenerator = gameObject.AddComponent<RoleGenerator>();
            itemControllar = gameObject.AddComponent<CubeControllar>();
            itemControllar.SetTransparentMaterial(transparentMaterial);

            item = itemGenerator.GeneratorCube();
            itemControllar.SetUser(item);
            
        }
    }
}