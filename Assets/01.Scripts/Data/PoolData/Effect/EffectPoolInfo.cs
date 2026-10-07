using System;
using UnityEngine;

[Serializable]
public class EffectPoolInfo
{
    [SerializeField] private int id;
    [SerializeField] private AfterImageObject prefab;

    public int ID => id;
    public AfterImageObject Prefab => prefab;
}
