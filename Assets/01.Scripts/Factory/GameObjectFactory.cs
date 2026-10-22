using System.Collections.Generic;
using UnityEngine;

public class GameObjectFactory : IFactory<PoolableGameObject>
{
    private readonly Dictionary<int, PoolableGameObject> prefabDic = new();

    public IPool Pool { get; }

    public GameObjectFactory()
    {
        Pool = new ObjectPool<PoolableGameObject>(this);
    }

    public void Register(int objectId, PoolableGameObject prefab)
    {
        if (prefab == null || prefabDic.ContainsKey(objectId))
            return;

        prefabDic.Add(objectId, prefab);
    }

    public PoolableGameObject Create(int objectId, Transform parent)
    {
        if (!prefabDic.TryGetValue(objectId, out PoolableGameObject prefab))
            return null;

        return Object.Instantiate(prefab, parent);
    }
}
