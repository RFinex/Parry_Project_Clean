using UnityEngine;

[CreateAssetMenu(fileName = "OldSamurai_Throw_Skill_Data", menuName = "Enemy/Skills/Old Samurai Throw")]
public class OldSamuraiThrowSkillData : EnemySkillData
{
    [Header("Projectile")]
    [SerializeField] private int objectId = 10003;
    [SerializeField] private Vector2 spawnOffset = new(0.5f, 0.3f);
    [SerializeField] private float playerBodyOffset = 0.6f;

    [Header("Attack Count")]
    [SerializeField] private int attackCount = 5;

    [Header("Attack Delay")]
    [SerializeField] private float throwDelay = 0.25f;
    [SerializeField] private float animEndDelay = 0.25f;

    public int ObjectId => objectId;
    public Vector2 SpawnOffset => spawnOffset;
    public float PlayerBodyOffset => playerBodyOffset;

    public float AttackCount => attackCount;

    public float ThrowDelay => throwDelay;
    public float AnimEndDelay => animEndDelay;

}
