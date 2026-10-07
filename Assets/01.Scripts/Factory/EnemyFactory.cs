using System.Collections.Generic;
using UnityEngine;

public interface IFactory
{
    IPool Pool { get; }
}

public interface IFactory<T> : IFactory where T : class, IPoolable
{
    void Register(int objectId, T prefab);
    T Create(int objectId, Transform parent);
}

public class EnemyFactory : IFactory<EnemyController>
{
    private readonly Dictionary<int, EnemyController> prefabDic = new Dictionary<int, EnemyController>();

    public IPool Pool { get; }

    public EnemyFactory()
    {
        Pool = new ObjectPool<EnemyController>(this);
    }

    public void Register(int objectId, EnemyController prefab)
    {
        if (prefab == null)
            return;

        if (prefabDic.ContainsKey(objectId))
            return;

        prefabDic.Add(objectId, prefab);
    }

    public EnemyController Create(int objectId, Transform parent)
    {
        if (!prefabDic.TryGetValue(objectId, out EnemyController prefab))
            return null;

        return Object.Instantiate(prefab, parent);
    }
}
