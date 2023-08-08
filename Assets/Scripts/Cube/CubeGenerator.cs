using Assets.scripts;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class CubeGenerator : MonoBehaviour
{
    private List<GameObject> generateList = new List<GameObject>();
    Transform targetPoint;
    public int maxGeneratorCount = 2; //方块上生成的最大角色数
    public float roleOffest = 1f; // 角色距离方块的偏移量(垂直)
    private void Awake()
    {
        Initial();
    }
    private void Start()
    {
        Initial();
    }
    /** 使用真随机生成指定范围之内的整数(0 - range-1) **/
    private int GetRandomValue(int range)
    {
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] randomBytes = new byte[4]; //4个字节是一个32位整数
        rng.GetBytes(randomBytes);
        int randomInt = BitConverter.ToInt32(randomBytes, 0);
        int res = (randomInt & 0x7FFFFFFF) % (range);

        return res;
    }
    public GameObject GeneratorCube()
    {
        int value, maxCount = maxGeneratorCount;
        value = GetRandomValue(generateList.Count);
        //从世界坐标系转移到局部坐标系
        GameObject obj = Instantiate(generateList[value], targetPoint.position, targetPoint.rotation, transform);
        //GameObject obj = Instantiate(generateList[value], targetPoint.position, Quaternion.identity);
        
        // 为生成的方块随机地添加角色
        foreach (Transform child in obj.transform)
        {
            int res = GetRandomValue(10);
            if (res > 5)
            {
                maxCount--;
                Vector3 tmp = child.position;
                GameObject role = GameManager._instance.roleGenerator.GeneratorRole(0, new Vector3(tmp.x, tmp.y + roleOffest, tmp.z));
                role.transform.parent = child;
            }
            if (maxCount == 0)
                break;
        }

        return obj;
    }

    public void Initial()
    {
        targetPoint = GameObject.Find("GeneratorPoint").transform;
        for (int i = 1; i <= 7; i++)
        {
            GameManager._instance.prefabAssests.TryGetValue("item" + i, out GameObject obj);
            generateList.Add(obj);
        }
    }
}
