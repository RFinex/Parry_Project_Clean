using UnityEngine;

public class BlackBoard
{

}

public class PlayerBlackBoard : BlackBoard
{
    public PlayerMovement movement = new PlayerMovement();

    public PlayerSkill skill = new PlayerSkill();

    public PlayerStats stats;

    public Rigidbody2D rb;

    public PlayerAnimator animator;

    public PlayerAfterImage afterImage;
}

public class EnemyBlackBoard : BlackBoard
{
    public Rigidbody2D rb;

    public SpriteRenderer sr;

    public EnemyStats stats;

    public EnemyCombat combat;
}