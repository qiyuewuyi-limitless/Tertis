using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Object
{
    private List<GameObject> pool = new List<GameObject>();
    private GameObject prefab;
    
    public ObjectPool(GameObject prefab,int initialSize = 50)
    {
        this.prefab = prefab;
        for (int i = 0; i < initialSize; i++)
        {
            AddToPool(CreateNewObject());
        }
    }
    private GameObject CreateNewObject()
    {
        GameObject newObj = Object.Instantiate(prefab); // ¿ËÂ¡¶ÔÏó
        return newObj;
    }
     private void AddToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Add(obj);
    }
    public GameObject GetObject()
    {
        GameObject obj;
        if(pool.Count > 0)
        {
            obj = pool[pool.Count - 1];
            pool.RemoveAt(pool.Count - 1);
        }
        else
        {
            obj = CreateNewObject();
        }
        obj.SetActive(true);
        return obj;
    }
    public void ReturnObject(GameObject obj)
    {
        AddToPool(obj);
    }
}
