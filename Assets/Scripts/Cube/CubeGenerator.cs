using Assets.scripts;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class CubeGenerator : MonoBehaviour
{
    private List<GameObject> generateList = new List<GameObject>();
    public int maxGeneratorCount = 2; //���������ɵ�����ɫ��
    public float roleOffest = 1f; // ��ɫ���뷽���ƫ����(��ֱ)
    private void Awake()
    {
        Initial();
    }
    private void Start()
    {
        Initial();
    }
    /** ʹ�����������ָ����Χ֮�ڵ�����(0 - range-1) **/
    private int GetRandomValue(int range)
    {
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] randomBytes = new byte[4]; //4���ֽ���һ��32λ����
        rng.GetBytes(randomBytes);
        int randomInt = BitConverter.ToInt32(randomBytes, 0);
        int res = (randomInt & 0x7FFFFFFF) % (range);

        return res;
    }
    public GameObject GeneratorCube(Vector3 point)
    {
        int value, maxCount = maxGeneratorCount;
        value = GetRandomValue(generateList.Count);
        //����������ϵת�Ƶ��ֲ�����ϵ
        GameObject obj = Instantiate(generateList[value], point, transform.rotation, transform);
        //GameObject obj = Instantiate(generateList[value], targetPoint.position, Quaternion.identity);
        
        // Ϊ���ɵķ����������ӽ�ɫ
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
        for (int i = 1; i <= 7; i++)
        {
            GameManager._instance.prefabAssests.TryGetValue("item" + i, out GameObject obj);
            generateList.Add(obj);
        }
    }
}
