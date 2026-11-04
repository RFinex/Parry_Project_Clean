using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class GoblinNormalAttackState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
        if (owner is not Goblin goblin || enemyBoard == null)
            return;

        EnterToken(owner.DestroyToken);
    }

    public override void Exit(EnemyController owner)
    {
        ExitToken();
    }
}
