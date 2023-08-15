using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T: UnityEngine.Object
{
    private List<T> pool = new List<T>();
    private T prefab;
    
    public ObjectPool(T prefab,int initialSize = 20)
    {
        this.prefab = prefab;
        for (int i = 0; i < initialSize; i++)
        {
            AddToPool(CreateNewObject());
        }
    }
    private T CreateNewObject()
    {
        T newObj = Object.Instantiate(prefab); // ¿ËÂ¡¶ÔÏó
        return newObj;
    }
     private void AddToPool(T obj)
    {
        pool.Add(obj);
    }

    public T GetObject()
    {
        T obj;
        if(pool.Count > 0)
        {
            obj = pool[pool.Count - 1];
            pool.RemoveAt(pool.Count - 1);
        }
        else
        {
            obj = CreateNewObject();
        }
        return obj;
    }
    public void ReturnObject(T obj)
    {
        AddToPool(obj);
    }
}
