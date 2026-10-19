using UnityEngine;

[CreateAssetMenu(fileName = "BanditJumpAttackSkillData", menuName = "Enemy/Skills/Bandit Jump Attack")]
public class BanditJumpAttackSkillData : EnemySkillData
{
    [SerializeField] private float horizontalSpeed = 4f;
    [SerializeField] private float jumpForce = 8f;

    public float HorizontalSpeed => horizontalSpeed;
    public float JumpForce => jumpForce;
}
