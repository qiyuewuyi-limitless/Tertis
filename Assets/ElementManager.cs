using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementManager : MonoBehaviour
{
    /* 常量区 */
    const int width = 10;
    const int height = 20;
    const int elementCount = 7;
    /* 初始化变量 */
    public Vector3Int unlimitPlace = new(-999, 0, 0);
    public Vector3Int createPos = new(4, 18, 0);
    private Vector2Int bounddSize = new(width, height);
    public RectInt Bounds
    {
        get
        {
            return new RectInt(Vector2Int.zero, bounddSize);
        }
        private set { }
    }

    public GameObject[,] map;
    public bool[,] ghostMap;

    public ElementData[] elements;
    public GameObject[] elementPrefabs;
    
    public Element ActiveElement { get; private set; }
    public Ghost ActiveGhost { get; private set; }

    private ObjectPool<GameObject> cubePool;
    private List<GameObject> gameObjetsForActiveElement;

    private void Awake()
    {
        elements = new ElementData[elementCount];
        //elementPrefabs = new GameObject[elementCount];
        gameObjetsForActiveElement = new();
        map = new GameObject[height, width];
        ghostMap = new bool[height, width];

        for (int i = 0; i < elements.Length; i++)
            elements[i].Initialize(i,elementPrefabs[i]);

        ActiveElement = GetComponent<Element>();
        ActiveGhost = GetComponent<Ghost>();
    }
    private void Start()
    {
        CreateElement();
    }
    public void CreateElement()
    {
        int random = Random.Range(0, elements.Length);
        //int random = 1;
        ElementData data = this.elements[random];

        ActiveElement.Initialize(this, createPos, data);
        FirstSet(ActiveElement);
    }

    public void FirstSet(Element element)
    {
        gameObjetsForActiveElement.Clear();

        for (int i = 0; i < element.Cells.Length; i++)
        {
            Vector3Int pos = element.Cells[i] + element.Position;
            Vector3 globalPos = pos + transform.localPosition;
            GameObject temp = Instantiate(element.Data.prefab, globalPos, Quaternion.identity, transform);
            gameObjetsForActiveElement.Add(temp);
            map[pos.y, pos.x] = temp;
        }
    }
    public void Set(Element element)
    {
        for (int i = 0; i < element.Cells.Length; i++)
        {
            Vector3Int pos = element.Cells[i] + element.Position;
            gameObjetsForActiveElement[i].transform.localPosition = pos;
            map[pos.y,pos.x] = gameObjetsForActiveElement[i];
        }    
    }
    public void Set(Ghost ghost)
    {
        for (int i = 0; i < ghost.CellsGameObject.Length; i++)
        {
            Vector3Int pos = ghost.Cells[i] + ghost.Position;
            ghostMap[pos.y, pos.x] = true;
        }
    }
    public void Clear(Element element)
    {
        for (int i = 0; i < element.Cells.Length; i++)
        {
            Vector3Int pos = element.Cells[i] + element.Position;
            map[pos.y, pos.x].transform.localPosition = unlimitPlace;
            map[pos.y, pos.x] = null;
        }
    }
    public void Clear(Ghost ghost)
    {
        for (int i = 0; i < ghost.Cells.Length; i++)
        {
            Vector3Int pos = ghost.Cells[i] + ghost.Position;
            ghostMap[pos.y, pos.x] = false;
        }
    }
    public bool CheckoutNewPositon(Element element,Vector3Int newPostion)
    {
        RectInt bounds = Bounds;
        for (int i = 0; i < element.Cells.Length; i++)
        {
            Vector3Int pos = element.Cells[i] + newPostion;

            if (!bounds.Contains((Vector2Int)pos))
                return false;
            if (map[pos.y, pos.x])
                return false;
        }
        return true;
    }
    public bool CheckoutNewPositon(Ghost ghost, Vector3Int newPostion)
    {
        // 方案一：ghost map
        // 方案二：屏蔽ActiveElement
        RectInt bounds = Bounds;
        for (int i = 0; i < ghost.Cells.Length; i++)
        {
            Vector3Int pos = ghost.Cells[i] + newPostion;

            if (!bounds.Contains((Vector2Int)pos))
                return false;
            if (ghostMap[pos.y, pos.x])
                return false;
        }
        return true;
    }

    public int? SelectMinRowOfFull()
    {
        for (int row = 0; row < map.GetLength(0); row++)
        {
            bool isFull = true;

            for (int col = 0; col < map.GetLength(1); col++)
                if (!map[row, col])
                {
                    isFull = false;
                    break;
                }

            if (isFull)
                return row;
        }

        return null;
    }
    public void LineClear(int row)
    {
        for(int col = 0; col < width; col++)
        {
            Destroy(map[row, col]);
            ghostMap[row, col] = false;
        }

        while(row < height - 1)
        {
            for(int col = 0; col < width; col++)
            {
                GameObject tempMap = map[row + 1, col];
                bool tempGhost = ghostMap[row + 1, col];
                if(tempMap)
                    tempMap.transform.localPosition += Vector3Int.down;
                map[row, col] = tempMap;
                ghostMap[row, col] = tempGhost;
            }
            row++;
        }
    }
    public void ClearLines()
    {
        int? row = SelectMinRowOfFull();
        while (row != null)
        {
            LineClear((int)row);
            row = SelectMinRowOfFull();
        }
    }
}
