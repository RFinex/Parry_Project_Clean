using UnityEngine;
using System.Collections.Generic;



[CreateAssetMenu(fileName = "EnemyPoolDataBase", menuName = "Pool/EnemyPoolDataBase")]
public class EnemyPoolDataBase : PoolDataBase
{
    [SerializeField] private List<EnemyPoolInfo> enemyList;

    public IReadOnlyList<EnemyPoolInfo> EnemyList => enemyList;

    /// <summary>
    /// Info에 등록된 적 정보를 풀에 저장
    /// </summary>
    /// <param name="manager"> ObjectPoolManager </param>
    public override void Register(ObjectPoolManager manager)
    {
        EnemyFactory factory = new EnemyFactory();

        foreach (EnemyPoolInfo info in enemyList)
        {
            manager.AddPool(info.ID, info.Prefab, factory, manager.ObjectPoolParent);
        }
    }
}
