using UnityEngine;

public class UIBase : MonoBehaviour, IPoolable
{
    private IPool pool;
    private int objectId;

    [SerializeField] protected UIName uiName;

    public void SetPool(IPool pool, int objectId)
    {
        this.pool = pool;
        this.objectId = objectId;
    }

    public void ReturnPool()
    {
        pool?.Return(this, objectId);
    }

    public virtual void InitPool()
    {

    }

    public virtual void InitPoolReturn()
    {

    }
}
