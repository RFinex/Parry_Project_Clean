using UnityEngine;

[CreateAssetMenu(fileName = "Attack_Data_Goblin", menuName = "Enemy/Goblin Attack")]
public class GoblinAttackData : EnemyAttackData
{
    [SerializeField] private float normalAttackRange = 1.5f;

    public float NormalAttackRange => normalAttackRange;
}
