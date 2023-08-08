using Assets.scripts;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoleManager : MonoBehaviour
{
    public static RoleManager _instance;

    /** Start阶段的role必须信息 */
    private Vector3 targetZero;

    /** Battle阶段的role必须信息 */
    public List<EnemyControllar> enemys;

    public TextMeshProUGUI player;
    public TextMeshProUGUI enemy;
    public int maxHealth = 100;
    public int enemyHealth = 20;

    private void Awake()
    {
        _instance = this;
        Initial();
    }

    public void HandleFullRow(GameObject row)
    {
        List<Transform> roles = new();

        foreach (Transform child in row.transform) //child为方块
            if (child.childCount != 0)
                roles.Add(child.GetChild(0));

        roles.Sort(new ComparerPositionX());

        foreach (Transform role in roles)
        {
            bool trigger = roles.IndexOf(role) == roles.Count - 1;
            Vector3 target = new(targetZero.x, role.position.y, role.position.z);
            role.GetComponent<RoleControllar>().EnableStartStage(target, trigger); //激活角色
        }
    }
    public void RemoveCubs(Transform ancestor)
    {
        GameManager._instance.RemoveCubs(ancestor);
    }

    public EnemyControllar FindRecentEnemy(RoleControllar role)
    {
        EnemyControllar enemy = null;
        float distance = role.data.radius;
        foreach (EnemyControllar enemyTmp in enemys)
        {
            float distanceTmp = Vector3.Distance(role.transform.position, enemyTmp.transform.position);
            if (distanceTmp <= distance)
            {
                distance = distanceTmp;
                enemy = enemyTmp;
            }
        }
        return enemy;
    }

    private void Initial()
    {
        targetZero = GameObject.Find("StartTarget").transform.position;
        enemys = new();
        player = GameObject.Find("Canvas").transform.Find("Player").Find("Text").GetComponent<TextMeshProUGUI>();
        enemy = GameObject.Find("Canvas").transform.Find("Enemy").Find("Text").GetComponent<TextMeshProUGUI>();
        player.text = "Health:" + maxHealth;
        enemy.text = "Enemy Health:" + enemyHealth;
    }
}
