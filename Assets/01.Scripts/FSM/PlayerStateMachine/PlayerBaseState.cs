using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState : BaseState<PlayerController>
{
    protected PlayerBlackBoard playerBoard;

    public virtual void HandleInput(PlayerInput input)
    {

    }

    public override void Initialized(BlackBoard blackBoard)
    {
        base.Initialized(blackBoard);

        if (blackBoard is PlayerBlackBoard board)
        {
            playerBoard = board;
        }
    }
}
