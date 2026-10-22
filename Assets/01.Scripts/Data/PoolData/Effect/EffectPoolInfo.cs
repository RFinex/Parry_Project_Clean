using System;
using UnityEngine;

[Serializable]
public class EffectPoolInfo
{
    [SerializeField] private int id;
    [SerializeField] private EffectBase prefab;

    public int ID => id;
    public EffectBase Prefab => prefab;
}
