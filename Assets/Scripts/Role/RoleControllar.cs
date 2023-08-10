using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

enum RoleState
{
    isEmpty, // 闲置
    isAgent, // nav流程
    isBattle // battle流程
}
public class RoleControllar : MonoBehaviour
{
    private RoleState myState;

    /* NavMesh设置 */
    protected NavMeshAgent roleAgent;
    protected Vector3 target;

    /** 数值: 攻击力，生命值，移动速度，阻挡数，攻击类型,索敌范围*/
    public RoleData data;

    private void Start()
    {
        Initial();
    }
    private void FixedUpdate()
    {
        switch (myState)
        {
            case RoleState.isAgent:
                AgentStage();
                break;
            case RoleState.isBattle:
                BattleStage();
                break;
        }
    }
    #region 阶段
    private void AgentStage()
    {
        if (!roleAgent.pathPending && roleAgent.remainingDistance <= 0.1f)
            ReachTarget();

        EnemyControllar enemy = RoleManager._instance.FindRecentEnemy(this);
        if (enemy)
        {
            BattleProcess process = gameObject.AddComponent<BattleProcess>();
            process.ProcessStart(this, enemy);
        }
    }
    private void BattleStage()
    {

    }
    #endregion

    #region 战斗
    public void BattleProcessStart()
    {
        roleAgent.isStopped = true;
        myState = RoleState.isBattle;
    }
    public void BattleProcessEnd()
    {
        roleAgent.isStopped = false;
        myState = RoleState.isAgent;
    }
    public void Death()
    {
        Destroy(gameObject);
    }
    #endregion

    #region 初始化
    private void InitialMeshAgent()
    {
        /* 参数设置 */
        if (roleAgent == null)
        {
            roleAgent = gameObject.AddComponent<NavMeshAgent>();
            roleAgent.radius = 0.5f;
            roleAgent.height = 1f;
            roleAgent.baseOffset = 0.5f;
            roleAgent.speed = 2f;
            roleAgent.angularSpeed = 0;
            roleAgent.acceleration = 400f;
            roleAgent.autoBraking = false;
        }
        
        /* 行为设置 */
        roleAgent.SetDestination(target);
    }
    protected void Initial()
    {
        myState = RoleState.isEmpty;
        data = new(2, 0, 3, 2, 1, 1.2f);
        target = GameObject.Find("EndTarget").transform.localPosition;
    }
    #endregion
    private void ReachTarget()
    {
        TextMeshProUGUI player = RoleManager._instance.enemy;
        int health = DataHandler.HandleStringWithInteger(player.text);
        player.text = "EnemyHealth:" + (health - 1);
        Destroy(gameObject);
    }
    public void EnableAgent()
    {
        StartCoroutine(TPCoroutine());
    }
    private IEnumerator TPCoroutine()
    {
        yield return new WaitForSeconds(1f); // TP动画播放时间
        transform.parent = RoleManager._instance.transform;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        myState = RoleState.isAgent;
        InitialMeshAgent();
        //callback.Invoke();
    }
}
