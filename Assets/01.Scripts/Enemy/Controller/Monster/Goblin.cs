using System;

public class Goblin : NormalEnemyController
{
    public override Type GetAttackStateType()
    {
        if (Target == null || blackBoard.attackDatas == null)
            return null;

        // 거리가 가까우면 근접 사거리에 맞는 근접 공격 선택
        if (IsAttackReady)
        {
            foreach (EnemyAttackData attackData in blackBoard.attackDatas)
            {
                if (attackData == null || attackData is EnemyProjectileAttackData)
                    continue;

                if (IsTargetInRange(attackData))
                    return typeof(NormalEnemyAttackState);
            }
        }        

        // 거리가 떨어져 있으면 사거리 및 쿨타임을 만족하는 투사체 공격 선택
        if (IsProjectileAttackReady)
        {
            foreach (EnemyAttackData attackData in blackBoard.attackDatas)
            {
                if (attackData is not EnemyProjectileAttackData)
                    continue;

                if (IsTargetInRange(attackData))
                    return typeof(NormalEnemyProjectileAttackState);
            }
        }

        return null;
    }
}
