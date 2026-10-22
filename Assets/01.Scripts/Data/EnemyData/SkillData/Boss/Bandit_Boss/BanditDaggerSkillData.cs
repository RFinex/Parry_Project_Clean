using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BanditDaggerSkillData", menuName = "Enemy/Skills/Bandit Dagger")]
public class BanditDaggerSkillData : EnemySkillData
{
    [Header("Projectile")]
    [SerializeField] private int objectId;
    [SerializeField] private Vector2 spawnOffset = new(0.5f, 0.3f);

    [Header("Attack Delay")]
    [Tooltip("단검 발사 시간")]
    [SerializeField]
    private List<float> shotTimes = new()
    {
        0.4f,
        0.6f,
        0.8f
    };

    [Tooltip("공격 애니메이션 길이")]
    [SerializeField] private float animationDuration = 1f;

    [Header("Attack Offset")]
    [Tooltip("플레이어 피벗 기준 조준 위치 조정")]
    [SerializeField] private float playerBodyOffset = 0.6f;

    public int ObjectId => objectId;
    public Vector2 SpawnOffset => spawnOffset;

    public List<float> ShotTimes => shotTimes;
    public float AnimationDuration => animationDuration;

    public float PlayerBodyOffset => playerBodyOffset;
}
