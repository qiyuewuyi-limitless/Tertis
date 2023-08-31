using UnityEngine;

public class Ghost : MonoBehaviour
{
    public ElementManager Manager { get; private set; }
    public Vector3Int[] Cells { get; private set; }
    public GameObject[] CellsGameObject { get; private set; }
    public Vector3Int Position { get; private set; }

    public GameObject prefab;

    private void Awake()
    {
        Manager = GetComponent<ElementManager>();

        Cells = new Vector3Int[4];
        CellsGameObject = new GameObject[Cells.Length];

        Vector3 globalPosition = Manager.unlimitPlace + transform.localPosition;
        for (int i = 0; i < CellsGameObject.Length; i++)
            CellsGameObject[i] = Instantiate(prefab, globalPosition, Quaternion.identity, transform);

    }
    private void LateUpdate()
    {
        Clone(Manager.ActiveElement);
        Drop();
        Rendering();
    }
    public void Clone(Element realitveElement)
    {
        this.Position = realitveElement.Position;
        for (int i = 0; i < realitveElement.Cells.Length; i++)
            Cells[i] = realitveElement.Cells[i];
    }
    private void Drop()
    {
        Vector3Int newPos = Position + Vector3Int.down;
        bool res = Manager.CheckoutNewPositon(this, newPos);

        while (res)
        {
            Position = newPos;
            newPos = Position + Vector3Int.down;
            res = Manager.CheckoutNewPositon(this, newPos);
        }

    }

    private void Rendering()
    {
        for (int i = 0; i < CellsGameObject.Length; i++)
        {
            Vector3Int pos = Cells[i] + Position;
            CellsGameObject[i].transform.localPosition = pos;
        }
    }
}
