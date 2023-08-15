using UnityEngine;

public enum ElementType
{
    I,
    O,
    T,
    J,
    S,
    Z,
    L,
}

[System.Serializable]
public struct ElementData
{
    public ElementType type;
    public GameObject prefab;
    public Vector2Int[] Cells { get; private set; }
    public Vector2Int[,] WallKicks { get; private set; }
    public void Initialize(int index,GameObject prefab)
    {        
        this.type = (ElementType)index;
        this.Cells = Data.Cells[type];
        this.WallKicks = Data.WallKicks[type];
        this.prefab = prefab;
    }
}

public class Element : MonoBehaviour
{
    public ElementManager Manager { get; private set; }
    public ElementData Data { get; private set; }
    public Vector3Int[] Cells { get; private set; }    
    public Vector3Int Position { get; private set; }
    public int RotateIndex { get; private set; }

    public float stepDelay = 0.4f; //自动延时
    public float lockDelay = 0.5f; //锁定延时

    private float lockTime;
    private float stepTime;
    
    public void Initialize(ElementManager manager,Vector3Int position,ElementData data)
    {
        this.Manager = manager;
        this.Position = position;
        this.Data = data;
        this.RotateIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;

        if (this.Cells == null)
            this.Cells = new Vector3Int[data.Cells.Length];
        
        for (int i = 0; i < Cells.Length; i++)
            this.Cells[i] = (Vector3Int)data.Cells[i];
    }

    private bool Move(Vector2Int translation) 
    {
        Vector3Int newPos = Position + (Vector3Int) translation;
        bool res = Manager.CheckoutNewPositon(this, newPos);
        
        if (res)
        {
            Position = newPos;
            lockTime = 0f;
        }

        return res;
    }

    private void MoveInput(Vector2Int translation)
    {
        Manager.Clear(this);
        Move(translation);
        Manager.Set(this);
    }

    private void HardDrop()
    {
        Manager.Clear(this);
        while (Move(Vector2Int.down))
            continue;

        Lock();
    }

    private int Wrap(int input,int min,int max)
    {
        return ((max - min) + input) % (max - min);
    }

    private void ApplyRotaionMatrix(int direction)
    {
        for (int i = 0; i < this.Cells.Length; i++)
        {
            Vector3 cell = this.Cells[i];
            int x, y;
            /* I,O方块的重心与其他方块不同，位于中心*/
            switch (this.Data.type)
            {
                case ElementType.I:
                case ElementType.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * global::Data.RotationMatrix[0] * direction) + (cell.y * global::Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * global::Data.RotationMatrix[2] * direction) + (cell.y * global::Data.RotationMatrix[3] * direction));
                    break;
                default:
                    x = Mathf.RoundToInt((cell.x * global::Data.RotationMatrix[0] * direction) + (cell.y * global::Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * global::Data.RotationMatrix[2] * direction) + (cell.y * global::Data.RotationMatrix[3] * direction));
                    break;
            }
            this.Cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private void Rotate(int direction)
    {
        Manager.Clear(this);

        int originalRotateIndex = this.RotateIndex; //旋转前的旋转状态

        this.RotateIndex = Wrap(this.RotateIndex + direction, 0, 4); // 旋转后的旋转状态
        
        ApplyRotaionMatrix(direction);

        if (!TryWallKicks(originalRotateIndex,direction))
        {
            this.RotateIndex = originalRotateIndex;
            ApplyRotaionMatrix(-direction);
        }

        Manager.Set(this);
    }

    private int GetWallKickIndex(int rotateIndex, int rotationDirection)
    {
        int wallKcikIndex = rotateIndex * 2;

        if (rotationDirection < 0)
        {
            wallKcikIndex--;
        }

        return Wrap(wallKcikIndex, 0, this.Data.WallKicks.GetLength(0));
    }

    private bool TryWallKicks(int rotateIndex,int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotateIndex, rotationDirection);

        for(int i = 0; i < this.Data.WallKicks.GetLength(1); i++)
        {
            Vector2Int translation = this.Data.WallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }
    private void Lock()
    {
        Manager.Set(this);
        Manager.Set(Manager.ActiveGhost);
        Manager.ClearLines(); 
        Manager.CreateElement();
    }
    private void Step()
    {
        stepTime = Time.time + stepDelay;
        
        Manager.Clear(this);       
        Move(Vector2Int.down);
        Manager.Set(this);

        if (lockTime >= lockDelay)
            Lock();

    }

    private void Awake()
    {
        /* 订阅事件 */
        TouchRegistrar.TouchMove += MoveInput;
        TouchRegistrar.TouchClick += Rotate;
        TouchRegistrar.TouchDrop += HardDrop;
    }

    private void Update()
    {
        lockTime += Time.deltaTime;

        if (Time.time >= stepTime)
            Step();
    }
}

