using UnityEngine;

public class PlayerDeadState : PlayerBaseState
{
    //private PlayerBlackBoard board;

    public override void Enter(PlayerController owner)
    {
        //if (blackBoard is PlayerBlackBoard board)
        //{
        //    this.board = board;
        //}

        playerBoard.animator.PlayDeath();
    }

    public override void Exit(PlayerController owner)
    {

    }
}
