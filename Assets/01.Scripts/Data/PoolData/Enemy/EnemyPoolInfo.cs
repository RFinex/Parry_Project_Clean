using System;
using UnityEngine;

[Serializable]
public class EnemyPoolInfo
{
    [SerializeField] private int id;
    [SerializeField] private EnemyController prefab;

    public int ID => id;
    public EnemyController Prefab => prefab;
}
