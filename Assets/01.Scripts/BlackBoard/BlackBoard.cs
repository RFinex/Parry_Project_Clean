using UnityEngine;

public class BlackBoard
{

}

public class PlayerBlackBoard : BlackBoard
{
    public PlayerMovement movement = new PlayerMovement();

    public PlayerSkill skill = new PlayerSkill();

    public Rigidbody2D rb;

    public PlayerAnimator animator;
}

public class EnemyBlackBoard : BlackBoard
{
    public Rigidbody2D rb;

}