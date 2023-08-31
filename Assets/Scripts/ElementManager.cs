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
    public float dissolveTime = 1f;
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
    public Material[] elementsMaterial;
    public GameObject elementPrefab;
    public Element ActiveElement { get; private set; }
    public Ghost ActiveGhost { get; private set; }

    private ObjectPool pool;
    private List<GameObject> gameObjetsForActiveElement;
    public List<int> randomList;

    private bool needClear;
    private MeshRenderer[] renderers;
    private void Awake()
    {
        elements = new ElementData[elementCount];
        for (int i = 0; i < elements.Length; i++)
        {
            //elementMaterial[i] = ??? ;
            elements[i].Initialize(i, elementsMaterial[i]);
        }

        map = new GameObject[height, width];
        ghostMap = new bool[height, width];
        
        //elementPrefab = ??? ;
        pool = new ObjectPool(elementPrefab);

        gameObjetsForActiveElement = new();
        randomList = new();
        renderers = new MeshRenderer[width];

        ActiveElement = GetComponent<Element>();
        ActiveGhost = GetComponent<Ghost>();
          
    }
    private void Start()
    {
        StartCoroutine(ClearLinesCoroutine());
        CreateElement();
    }
    public void CreateElement()
    {
        while(randomList.Count < 2)
            randomList.Add(Random.Range(0, elements.Length));
        //int random = Random.Range(0, elements.Length);
        //int random = 1;
        ElementData data = this.elements[randomList[0]];

        ActiveElement.Initialize(this, createPos, data);
        FirstSet(ActiveElement);
    }

    public void FirstSet(Element element)
    {
        gameObjetsForActiveElement.Clear();

        for (int i = 0; i < element.Cells.Length; i++)
        {
            Vector3Int pos = element.Cells[i] + element.Position;
            GameObject temp = pool.GetObject();
            
            temp.transform.parent = transform;
            temp.transform.localPosition = pos;
            temp.GetComponent<Renderer>().material = element.Data.material;

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
            pool.ReturnObject(map[row, col]);
            //Destroy(map[row, col]);
            ghostMap[row, col] = false;
        }
        Clear(ActiveElement);
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
        Set(ActiveElement);
    }
    private void SetDissloveRate(float value)
    {
        int shaderId = Shader.PropertyToID(name: "_CilpRate");
        foreach(MeshRenderer renderer in renderers)
        {
            foreach(Material material in renderer.materials)
            {
                material.SetFloat(shaderId, value);
            }
        }
    }
    private IEnumerator LineDissolve(int row)
    {
        for (int col = 0; col < width; col++)
        {
            renderers[col] = map[row, col].GetComponent<MeshRenderer>();
        }
        SetDissloveRate(0);
        float time = 0f;
        while(time < dissolveTime)
        {
            time += Time.deltaTime;
            //Mathf.Lerp(0,dissolveTime,time);
            SetDissloveRate(time / dissolveTime);
            yield return null;
        }
    }
    /** 自底向上 */
    public void ClearLines()
    {
        needClear = true;
/*        int? row = SelectMinRowOfFull();
        while (row != null)
        {
            LineClear((int)row);
            row = SelectMinRowOfFull();
        }*/
    }
    private IEnumerator ClearLinesCoroutine()
    {
        while (true)
        {
            if(!needClear)
                yield return null;
            else
            {
                int? row = SelectMinRowOfFull();
                while( row != null)
                {
                    StartCoroutine(LineDissolve((int)row));//控制shader参数完成动画效果
                    yield return new WaitForSeconds(dissolveTime);
                    LineClear((int)row);
                    row = SelectMinRowOfFull();
                }
                needClear = false;
            }
        }
    }
}
