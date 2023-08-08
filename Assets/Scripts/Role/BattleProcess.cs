using UnityEngine;

public class BattleProcess : MonoBehaviour
{
    private RoleControllar role;
    private EnemyControllar enemy;

    private void FixedUpdate()
    {
        ProcessIn();
    }

    #region 生命周期
    public void ProcessStart(RoleControllar role, EnemyControllar enemy)
    {
        this.role = role;
        this.enemy = enemy;
        role.BattleProcessStart();
        enemy.BattleProcessStart();
        RoleManager._instance.enemys.Remove(enemy);
    }
    private void ProcessIn()
    {
        if (role && role.data.health <= 0 && enemy && enemy.data.health <= 0)
        {
            ProcessEnd(0);
            return;
        }
        if (role && role.data.health <= 0)
            ProcessEnd(1);
        else if (enemy && enemy.data.health <= 0)
            ProcessEnd(2);
    }
    private void ProcessEnd(int id)
    {
        switch (id)
        {
            case 0:
                enemy.Death();
                role.Death();
                break;
            case 1:
                role.Death();
                enemy.BattleProcessEnd();
                RoleManager._instance.enemys.Add(enemy); // 归还
                break;
            case 2:
                enemy.Death();
                role.BattleProcessEnd();
                break;
        }
        Destroy(this); // 销毁
    }
    #endregion
}
