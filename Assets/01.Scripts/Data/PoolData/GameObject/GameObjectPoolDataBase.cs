using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameObjectPoolDataBase", menuName = "Pool/GameObjectPoolDataBase")]
public class GameObjectPoolDataBase : PoolDataBase
{
    [SerializeField] private List<GameObjectPoolInfo> objectList;
    public override void Register(ObjectPoolManager manager)
    {
        GameObjectFactory factory = new();

        foreach (GameObjectPoolInfo info in objectList)
        {
            if (info == null || info.Prefab == null)
                continue;

            manager.AddPool(info.Id, info.Prefab, factory, manager.ObjectPoolParent);
        }
    }
}
