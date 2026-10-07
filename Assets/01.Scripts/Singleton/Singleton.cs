using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    private static bool isQuit;

    public static T Instance
    {
        get
        {
            if (isQuit)
                return null;

            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();

                if (instance == null)
                {
                    var go = new GameObject(typeof(T).Name);
                    instance = go.AddComponent<T>();
                }
            }

            return instance;
        }
    }

    protected virtual bool DDOL => false;

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this as T;

        if (DDOL)
            DontDestroyOnLoad(gameObject);

        AwakeSingleton();
    }

    protected virtual void AwakeSingleton()
    {

    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void OnApplicationQuit()
    {
        isQuit = true;
    }
}
