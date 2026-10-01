using UnityEngine;

public class EnemyPatrolState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
        EnterToken();
    }

    public override void Exit(EnemyController owner)
    {
        ExitToken();
    }
}
