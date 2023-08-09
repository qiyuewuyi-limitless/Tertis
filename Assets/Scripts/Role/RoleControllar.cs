using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

enum RoleState
{
    isEmpty, // ����
    isAgent, // nav����
    isBattle // battle����
}
public class RoleControllar : MonoBehaviour
{
    private RoleState myState;

    /* NavMesh���� */
    protected NavMeshAgent roleAgent;
    protected Vector3 target;

    /** ��ֵ: ������������ֵ���ƶ��ٶȣ��赲������������,���з�Χ*/
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
    #region �׶�
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

    #region ս��
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

    #region ��ʼ��
    private void InitialMeshAgent()
    {
        /* �������� */
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
        
        /* ��Ϊ���� */
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
    public void EnableAgent(Action callback)
    {
        StartCoroutine(TPCoroutine(callback));
    }
    private IEnumerator TPCoroutine(Action callback)
    {
        yield return new WaitForSeconds(1f); // TP��������ʱ��
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        myState = RoleState.isAgent;
        InitialMeshAgent();
        callback.Invoke();
    }
}
