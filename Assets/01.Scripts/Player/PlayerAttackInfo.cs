using UnityEngine;

public struct PlayerAttackInfo
{
    public float damage;
    public Transform attacker;

    public PlayerAttackInfo(float damage, Transform attacker)
    {
        this.damage = damage;
        this.attacker = attacker;
    }
}
