using UnityEngine;

public class UIBase : MonoBehaviour, IPoolable
{
    private IPool pool;
    private int objectId;

    public void SetPool(IPool pool, int objectId)
    {
        this.pool = pool;
        this.objectId = objectId;
    }

    public void ReturnPool()
    {
        pool?.Return(this, objectId);
    }

    public void InitPool()
    {

    }

    public void InitPoolReturn()
    {

    }
}
