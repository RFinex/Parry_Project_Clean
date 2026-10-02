using UnityEngine;

public class BlackBoard
{

}

public class PlayerBlackBoard : BlackBoard
{
    public PlayerMovement movement = new PlayerMovement();

    public PlayerSkill skill = new PlayerSkill();

    public Rigidbody2D rb;
}

public class EnemyBlackBoard : BlackBoard
{

}