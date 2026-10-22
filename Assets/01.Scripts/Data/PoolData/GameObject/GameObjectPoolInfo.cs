using System;
using UnityEngine;

[Serializable]
public class GameObjectPoolInfo
{
    [SerializeField] private int id;
    [SerializeField] private PoolableGameObject prefab;

    public int Id => id;
    public PoolableGameObject Prefab => prefab;
}
