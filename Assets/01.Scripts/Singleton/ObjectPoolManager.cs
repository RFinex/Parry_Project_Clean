using UnityEngine;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

public interface IPoolable
{
    void SetPool(IPool pool, int objectId);
    void InitPool();
    void InitPoolReturn();
    void ReturnPool();
}

public interface IPool
{
    IPoolable Get(int objectId, Vector3 position, Transform parent = null);
    void Return(IPoolable item, int objectId);
    void Prewarm(int objectId, int poolSize, Transform parent);
}

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    protected override bool DDOL => true;

    [Header("Pool")]
    [SerializeField] private int poolSize = 10;
    [SerializeField] private List<PoolDataBase> dataBases;

    [Header("Pool Parent")]
    private Transform objectPoolParent;
    private Transform uiPoolParent;

    private readonly Dictionary<int, IPool> poolDic = new Dictionary<int, IPool>();

    public Transform ObjectPoolParent => objectPoolParent;
    public Transform UIPoolParent => uiPoolParent;

    protected override void Awake()
    {
        base.Awake();

        objectPoolParent = new GameObject("ObjectPoolParent").transform;
        objectPoolParent.SetParent(transform, false);

        // 비동기로 수정
        foreach (PoolDataBase dataBase in dataBases)
        {
            if (dataBase == null)
                continue;

            dataBase.Register(this);
        }
    }

    public void SetUIPoolParent(Transform parent)
    {
        uiPoolParent = parent;
    }

    /// <summary>
    /// Pool 등록
    /// </summary>
    /// <typeparam name="T"> 등록할 오브젝트 </typeparam>
    /// <param name="objectId"> 오브젝트 ID </param>
    /// <param name="factory"> 오브젝트 생성 Factory </param>
    /// <param name="prefab"> 오브젝트 프리팹 </param>
    public void AddPool<T>(int objectId, T prefab, IFactory<T> factory, Transform parent) where T : class, IPoolable
    {
        if (factory == null || prefab == null)
            return;

        if (poolDic.ContainsKey(objectId))
            return;

        // factory에 프리팹 등록
        factory.Register(objectId, prefab);

        // 해당 팩토리 담당 Pool에 poolSize 전달
        factory.Pool.Prewarm(objectId, poolSize, parent);

        // 오브젝트 ID와 Pool을 연결
        poolDic.Add(objectId, factory.Pool);
    }

    public T Get<T>(int objectId, Vector3 position) where T : MonoBehaviour, IPoolable
    {
        if (!poolDic.TryGetValue(objectId, out IPool pool))
            return null;

        return pool.Get(objectId, position) as T;
    }
}



public class ObjectPool<T> : IPool where T : MonoBehaviour, IPoolable
{
    // 하나의 풀에서 여러 오브젝트 관리
    private readonly Dictionary<int, Queue<T>> poolDic = new Dictionary<int, Queue<T>>();

    // 오브젝트 반납 시 돌아갈 위치
    private readonly Dictionary<int, Transform> parentDic = new Dictionary<int, Transform>();

    private readonly IFactory<T> factory;

    private readonly HashSet<T> pooledItems = new HashSet<T>();

    public ObjectPool(IFactory<T> factory)
    {
        this.factory = factory;
    }

    /// <summary>
    /// 오브젝트 미리 생성
    /// </summary>
    /// <param name="objectId"> 오브젝트 ID </param>
    /// <param name="poolSize"> 풀링 사이즈 </param>
    /// <param name="parent"> 부모 오브젝트 위치 </param>
    public void Prewarm(int objectId, int poolSize, Transform parent)
    {
        if (poolDic.ContainsKey(objectId))
            return;

        Transform poolParent = CreatePoolParent(objectId, parent);

        parentDic.Add(objectId, poolParent);

        Queue<T> queue = new Queue<T>(poolSize);

        poolDic.Add(objectId, queue);

        for (int i = 0; i < poolSize; i++)
        {
            T item = Create(objectId);

            if (item != null)
            {
                item.SetPool(this, objectId);
                item.gameObject.SetActive(false);
                queue.Enqueue(item);
                pooledItems.Add(item);
            }
        }
    }

    /// <summary>
    /// ID 별 부모 생성
    /// </summary>
    /// <param name="objectId"> 오브젝트 ID </param>
    /// <param name="parent"> 부모 오브젝트 위치 </param>
    /// <returns></returns>
    private Transform CreatePoolParent(int objectId, Transform parent)
    {
        GameObject parentObject = new GameObject($"Pool_{objectId}");

        Transform root = parentObject.transform;

        root.SetParent(parent, false);

        return root;
    }

    private T Create(int objectId)
    {
        if (!parentDic.TryGetValue(objectId, out Transform poolParent))
            return null;

        return factory.Create(objectId, poolParent);
    }

    public IPoolable Get(int objectId, Vector3 position, Transform parent = null)
    {
        if (!poolDic.TryGetValue(objectId, out Queue<T> queue))
            return null;

        T item;

        if (queue.Count > 0)
        {
            item = queue.Dequeue();
        }
        else
        {
            item = Create(objectId);

            if (item == null)
                return null;

            item.SetPool(this, objectId);
        }

        pooledItems.Remove(item);

        item.transform.SetParent(parent);

        item.transform.position = position;

        item.gameObject.SetActive(true);

        item.InitPool();

        return item;
    }

    public void Return(IPoolable item, int objectId)
    {
        if (item is not T target)
            return;

        if (!poolDic.TryGetValue(objectId, out Queue<T> queue))
            return;

        if (!pooledItems.Add(target))
            return;

        target.InitPoolReturn();

        if (parentDic.TryGetValue(objectId, out Transform poolParent))
        {
            target.transform.SetParent(poolParent, false);
        }

        target.gameObject.SetActive(false);

        queue.Enqueue(target);
    }
}