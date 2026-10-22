using UnityEngine;

public abstract class PoolableGameObject : MonoBehaviour, IPoolable
{
    private IPool pool;
    private int objectId;

    public virtual void SetPool(IPool pool, int objectId)
    {
        this.pool = pool;
        this.objectId = objectId;
    }

    public virtual void InitPool()
    {

    }

    public virtual void InitPoolReturn()
    {

    }

    public void ReturnPool()
    {
        pool?.Return(this, objectId);
    }    
}
