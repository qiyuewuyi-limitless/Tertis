using UnityEngine;
using UnityEngine.Scripting;

public class RoleData : Object
{
    public float attack;
    public float health;
    public float moveSpeed;
    public int blockCount;
    public int attackType;
    public float radius;

    [Preserve]
    public RoleData()
    {

    }

    public RoleData(float attack, float health, float moveSpeed, int blockCount, int attackType, float radius)
    {
        this.attack = attack;
        this.health = health;
        this.moveSpeed = moveSpeed;
        this.blockCount = blockCount;
        this.attackType = attackType;
        this.radius = radius;
    }
}
