using TMPro;
using UnityEngine;
using UnityEngine.AI;

enum EnemyState
{
    isAgent, //nav流程
    isBattle //battle流程
}
public class EnemyControllar : MonoBehaviour
{
    private EnemyState myState;

    private NavMeshAgent enemyAgent;
    Vector3 target;

    public EnemyData data;

    private void Awake()
    {
        Initial();
    }
    private void FixedUpdate()
    {
        switch (myState)
        {
            case EnemyState.isAgent:
                AgentStage();
                break;
            case EnemyState.isBattle:
                BattleStage();
                break;
        }
    }

    #region 阶段
    private void AgentStage()
    {
        if (Mathf.Abs(transform.position.z - target.z) <= 0.1)
            ReachTarget();

    }
    private void BattleStage()
    {

    }
    #endregion

    #region 战斗
    public void BattleProcessStart()
    {
        enemyAgent.isStopped = true;
        myState = EnemyState.isBattle;
    }
    public void BattleProcessEnd()
    {
        enemyAgent.isStopped = false;
        myState = EnemyState.isAgent;
    }
    public void Death()
    {
        RoleManager._instance.enemys.Remove(this);
        Destroy(gameObject);
    }
    #endregion

    #region 初始化
    private void InitialMeshAgent()
    {

        if (enemyAgent == null)
        {
            enemyAgent = gameObject.AddComponent<NavMeshAgent>();
            enemyAgent.radius = 0.5f;
            enemyAgent.height = 1f;
            enemyAgent.baseOffset = 0.5f;
            enemyAgent.speed = 2f;
            enemyAgent.angularSpeed = 0;
            enemyAgent.acceleration = 400f;
            enemyAgent.autoBraking = false;
        }

        target = GameObject.Find("StartTarget").transform.position;
        enemyAgent.SetDestination(target);
    }
    private void Initial()
    {
        RoleManager._instance.enemys.Add(this);

        data = new(2, 0, 1);
        myState = EnemyState.isAgent;
        InitialMeshAgent();
    }
    #endregion

    private void ReachTarget()
    {
        RoleManager._instance.enemys.Remove(this);
        TextMeshProUGUI player = RoleManager._instance.player;
        int health = DataHandler.HandleStringWithInteger(player.text);
        player.text = "Health:" + (health - 1);
        Destroy(gameObject);
    }
}
