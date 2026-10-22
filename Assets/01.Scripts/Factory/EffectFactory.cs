using System.Collections.Generic;
using UnityEngine;

public class EffectFactory : IFactory<EffectBase>
{
    private readonly Dictionary<int, EffectBase> prefabDic = new();

    public IPool Pool { get; }

    public EffectFactory()
    {
        Pool = new ObjectPool<EffectBase>(this);
    }

    public void Register(int objectId, EffectBase prefab)
    {
        if (prefab == null || prefabDic.ContainsKey(objectId))
            return;

        prefabDic.Add(objectId, prefab);
    }

    public EffectBase Create(int objectId, Transform parent)
    {
        if (!prefabDic.TryGetValue(objectId, out EffectBase prefab))
            return null;

        return Object.Instantiate(prefab, parent);
    }    
}
