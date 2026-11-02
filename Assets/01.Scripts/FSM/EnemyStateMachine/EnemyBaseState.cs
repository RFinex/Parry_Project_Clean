using UnityEngine;

public abstract class EnemyBaseState : BaseState<EnemyController>
{
    protected EnemyBlackBoard enemyBoard;

    public override void Initialized(BlackBoard blackBoard)
    {
        base.Initialized(blackBoard);

        if (blackBoard is EnemyBlackBoard board)
            enemyBoard = board;
    }
}
