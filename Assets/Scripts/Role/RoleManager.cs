using Assets.scripts;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoleManager : MonoBehaviour
{
    public static RoleManager _instance;

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

    public void Method()
    {

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
        enemys = new();
        player = GameObject.Find("Canvas").transform.Find("Player").Find("Text").GetComponent<TextMeshProUGUI>();
        enemy = GameObject.Find("Canvas").transform.Find("Enemy").Find("Text").GetComponent<TextMeshProUGUI>();
        player.text = "Health:" + maxHealth;
        enemy.text = "Enemy Health:" + enemyHealth;
    }
}
