using UnityEngine;

public class EffectBase : MonoBehaviour, IPoolable
{
    private IPool pool;
    private int objectId;

    public virtual void InitPool()
    {
        gameObject.SetActive(true);
    }

    public virtual void InitPoolReturn()
    {

    }

    public void ReturnPool()
    {
        pool?.Return(this, objectId);
    }

    public virtual void SetPool(IPool pool, int objectId)
    {
        this.pool = pool;
        this.objectId = objectId;
    }
}
