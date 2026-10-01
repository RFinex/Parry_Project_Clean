using UnityEngine;

public abstract class EnemyController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] protected EnemyBaseData data;

    [Header("Stats")]
    [SerializeField] protected EnemyStats stats;

    private float moveSpeed;

    protected Rigidbody2D rb;
    
    public float FrontDir {  get; private set; }

    private StateMachine<EnemyController> stateMachine;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        FrontDir = 1f;

        Initialized();
    }

    private void Initialized()
    {
        stateMachine = new StateMachine<EnemyController>(this);

        stats.Initialized(data);
    }

    protected abstract void Move();
    
    protected void SetFrontDir(float direction)
    {
        if (direction == 0f)
            return;

        FrontDir = Mathf.Sign(direction);
    }

}
