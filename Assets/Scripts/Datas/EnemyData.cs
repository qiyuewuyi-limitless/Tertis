using UnityEngine.Scripting;

public class EnemyData
{
    public float attack;
    public float health;
    public float moveSpeed;

    [Preserve]
    public EnemyData()
    {
    }

    public EnemyData(float attack, float health, float moveSpeed)
    {
        this.attack = attack;
        this.health = health;
        this.moveSpeed = moveSpeed;
    }
}
