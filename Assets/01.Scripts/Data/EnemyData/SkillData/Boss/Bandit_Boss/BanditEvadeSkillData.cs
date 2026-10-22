using UnityEngine;

[CreateAssetMenu(fileName = "BanditBossEvadeSkillData", menuName = "Enemy/Skills/Bandit Evade")]
public class BanditEvadeSkillData : EnemySkillData
{
    [Header("Evade")]
    [SerializeField] private float horizontalSpeed = 5f;
    [SerializeField] private float jumpForce = 2f;

    public float HorizontalSpeed => horizontalSpeed;
    public float JumpForce => jumpForce;
}
