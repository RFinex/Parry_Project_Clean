using System.Collections.Generic;
using UnityEngine;

public class UIFactory : IFactory<UIBase>
{
    private readonly Dictionary<int, UIBase> prefabDic = new Dictionary<int, UIBase>();

    public IPool Pool { get; }

    public UIFactory()
    {
        Pool = new ObjectPool<UIBase>(this);
    }

    public void Register(int objectId, UIBase prefab)
    {
        if (prefab == null)
            return;

        if (prefabDic.ContainsKey(objectId))
            return;

        prefabDic.Add(objectId, prefab);
    }

    public UIBase Create(int objectId, Transform parent)
    {
        if (!prefabDic.TryGetValue(objectId, out UIBase prefab))
            return null;

        return Object.Instantiate(prefab, parent);
    }
}
