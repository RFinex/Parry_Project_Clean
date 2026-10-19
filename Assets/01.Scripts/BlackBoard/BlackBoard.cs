using UnityEngine;
using System.Collections.Generic;

public class BlackBoard
{

}

public class PlayerBlackBoard : BlackBoard
{
    public PlayerMovement movement = new PlayerMovement();
    public PlayerSkill skill = new PlayerSkill();


    public PlayerStats stats;
    public PlayerCombat combat;

    public Rigidbody2D rb;
    public Transform playerTransform;
    public GameObject executeTarget;

    public PlayerAnimator animator;
    public PlayerAfterImage afterImage;

}

public class EnemyBlackBoard : BlackBoard
{
    public bool combatStarted;

    public Rigidbody2D rb;
    public SpriteRenderer sr;
    public EnemyStats stats;
    public EnemyCombat combat;
    public EnemyAnimator animator;

    public LayerMask playerLayer;

    public List<EnemySkill> skills = new();
    public EnemySkill currentSkill;
}