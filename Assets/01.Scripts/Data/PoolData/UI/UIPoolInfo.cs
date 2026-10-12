using System;
using UnityEngine;

[Serializable]
public class UIPoolInfo
{
    [SerializeField] private UIName id;
    [SerializeField] private UIBase prefab;

    public UIName ID => id;
    public UIBase Prefab => prefab;
}
