using System.Collections.Generic;
using UnityEngine;

public class EffectFactory : IFactory<AfterImageObject>
{
    private readonly Dictionary<int, AfterImageObject> prefabDic = new Dictionary<int, AfterImageObject>();

    public IPool Pool { get; }

    public EffectFactory()
    {
        Pool = new ObjectPool<AfterImageObject>(this);
    }

    public void Register(int objectId, AfterImageObject prefab)
    {
        if (prefab == null)
            return;

        if (prefabDic.ContainsKey(objectId))
            return;

        prefabDic.Add(objectId, prefab);
    }

    public AfterImageObject Create(int objectId, Transform parent)
    {
        if (!prefabDic.TryGetValue(objectId, out AfterImageObject prefab))
            return null;

        return Object.Instantiate(prefab, parent);
    }    
}
