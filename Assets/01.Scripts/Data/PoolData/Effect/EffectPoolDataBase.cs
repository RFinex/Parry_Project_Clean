using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectPoolDataBase", menuName = "Pool/EffectPoolDataBase")]
public class EffectPoolDataBase : PoolDataBase
{
    [SerializeField] private List<EffectPoolInfo> effectList;

    public IReadOnlyList<EffectPoolInfo> EffectList => effectList;

    public override void Register(ObjectPoolManager manager)
    {
        EffectFactory factory = new EffectFactory();

        foreach (EffectPoolInfo info in effectList)
        {
            manager.AddPool(info.ID, info.Prefab, factory, manager.ObjectPoolParent);
        }
    }
}
