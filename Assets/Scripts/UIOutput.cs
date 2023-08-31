using TMPro;
using UnityEngine;

public class UIOutput : MonoBehaviour
{
    public TextMeshProUGUI pre;
    public TextMeshProUGUI after;
    public ElementManager Manager { get; private set; }
    private void Awake()
    {
        Manager = GetComponent<ElementManager>();
    }
    private void Update()
    {
        pre.text = ((ElementType)Manager.randomList[0]).ToString();
        after.text = ((ElementType)Manager.randomList[1]).ToString();
    }
}
