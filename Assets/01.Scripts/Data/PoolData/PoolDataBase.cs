using UnityEngine;

public abstract class PoolDataBase : ScriptableObject
{
    public abstract void Register(ObjectPoolManager manager);
}