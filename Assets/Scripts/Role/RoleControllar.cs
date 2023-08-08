using TMPro;
using UnityEngine;
using UnityEngine.AI;

enum RoleState
{
    isEmpty, // ����
    isStart, // start����
    isAgent, // nav����
    isBattle // battle����
}
public class RoleControllar : MonoBehaviour
{
    private RoleState myState;

    /* ʹ��NavMesh֮ǰ */
    private Vector3 targetZero;
    private float frameSpeed;
    private bool triggerFlag;

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
            case RoleState.isStart:
                StartStage();
                break;
            case RoleState.isAgent:
                AgentStage();
                break;
            case RoleState.isBattle:
                BattleStage();
                break;
        }
    }

    #region �׶�
    private void StartStage()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetZero, frameSpeed);
        if (Mathf.Abs(transform.position.x - targetZero.x) <= 0.1)
        {
            Transform ancestor = transform.parent.parent;
            transform.parent = RoleManager._instance.transform;

            myState = RoleState.isAgent;
            InitialMeshAgent();
            
            if (triggerFlag)
            {
                RoleManager._instance.RemoveCubs(ancestor);//��ɫ-������-��
                triggerFlag = false;
            }

        }

    }
    private void AgentStage()
    {
        if (Mathf.Abs(transform.position.z - target.z) <= 0.1)
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
        target = GameObject.Find("EndTarget").transform.position;
        roleAgent.SetDestination(target);

    }
    protected void Initial()
    {
        myState = RoleState.isEmpty;
        data = new(2, 0, 3, 2, 1, 1.2f);
        /* ��������ת��Ϊ֡���� */
        frameSpeed = data.moveSpeed * (Time.fixedDeltaTime / 1);
        triggerFlag = false;
    }
    #endregion
    private void ReachTarget()
    {
        TextMeshProUGUI player = RoleManager._instance.enemy;
        int health = DataHandler.HandleStringWithInteger(player.text);
        player.text = "EnemyHealth:" + (health - 1);
        Destroy(gameObject);
    }
    public void EnableStartStage(Vector3 target, bool flag)
    {
        myState = RoleState.isStart;
        targetZero = target;
        triggerFlag = flag;
    }
}
