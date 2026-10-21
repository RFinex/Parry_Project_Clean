using UnityEngine;

public class BTState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
        EnterToken(owner.DestroyToken);

        if (owner is EliteEnemyController elite)
            elite.StartBT(token.Token);
    }

    public override void Exit(EnemyController owner)
    {
        if (owner is EliteEnemyController elite)
            elite.StopBT();

        ExitToken();
    }
}
