using System;
using UnityEngine;

[Serializable]
public class UIPoolInfo
{
    [SerializeField] private int id;
    [SerializeField] private UIBase prefab;

    public int ID => id;
    public UIBase Prefab => prefab;
}
