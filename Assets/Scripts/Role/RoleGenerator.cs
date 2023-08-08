using Assets.scripts;
using System.Collections.Generic;
using UnityEngine;

public class RoleGenerator : MonoBehaviour
{
    private List<GameObject> generatorList = new();
    public int roleTypeCount = 2;
    public int enemyCounts;
    public float generatorTime = 1f;
    private float generatorTimer = 0f;
    private Vector3 enemyBirthplace;
    private void Awake()
    {
        Initial();
    }

    private void FixedUpdate()
    {
        generatorTimer += Time.fixedDeltaTime;
        if(generatorTimer >= generatorTime && enemyCounts > 0)
        {
            generatorTimer = 0;
            GeneratorRole(1, enemyBirthplace); // 1表示敌方单位
            enemyCounts--;
        }
    }

    public GameObject GeneratorRole(int id, Vector3 position)
    {
        return Instantiate(generatorList[id], position, Quaternion.identity);
    }
    
    private void Initial()
    {
        enemyCounts = 10;
        enemyBirthplace = GameObject.Find("EndTarget").transform.position;
        GameManager._instance.prefabAssests.TryGetValue("role", out GameObject role);
        GameManager._instance.prefabAssests.TryGetValue("enemy", out GameObject enemy);
        generatorList.Add(role);
        generatorList.Add(enemy);
    }
} 
