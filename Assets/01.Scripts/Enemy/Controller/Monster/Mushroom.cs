using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

// [수정] 공격 진행은 일반 적 공통 컨트롤러에서 상속
public class Mushroom : NormalEnemyController
{
    [Header("Attack Data")]
    [SerializeField] private MushroomAttackData attack1Data;
    [SerializeField] private MushroomAttackData attack2Data;

    public MushroomAttackData Attack1Data => attack1Data;
    public MushroomAttackData Attack2Data => attack2Data;

    private int attackIndex;

    protected override void AddStates()
    {
        base.AddStates();
        EnemyAddState<MushroomAttack1State>();
        EnemyAddState<MushroomAttack2State>();
    }

    public override async UniTask<bool> AttackAsync(EnemyAttackData attackData, CancellationToken ctk)
    {
        try
        {
            return await base.AttackAsync(attackData, ctk);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            attackIndex = attackIndex == 0 ? 1 : 0;
        }
    }

    public override Type GetAttackStateType()
    {
        return attackIndex == 1 ? typeof(MushroomAttack2State) : typeof(MushroomAttack1State);
    }

    protected override void ResetPoolState()
    {
        base.ResetPoolState();
        attackIndex = 0;
    }
}
