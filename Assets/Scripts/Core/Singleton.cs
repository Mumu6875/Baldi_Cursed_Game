using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    public static T Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                return null;
            }

            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null && Application.isPlaying)
                {
                    instance = CreateInstance();
                }
            }
            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod]
    private static void Init() => Application.quitting += () => isApplicationQuitting = true;

    protected virtual void Awake()
    {
        isApplicationQuitting = false;

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this as T;
        OnAwake();

        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnDestroy() => instance = null;

    protected virtual void OnAwake() { }

    public static bool TryGetInstance(out T result)
    {
        if (isApplicationQuitting)
        {
            result = null;
            return false;
        }
        result = Instance;
        return result != null;
    }

    private static T CreateInstance()
    {
        GameObject obj = new GameObject(typeof(T).Name);
        DontDestroyOnLoad(obj);
        return obj.AddComponent<T>();
    }

    private static T instance;
    public static bool isApplicationQuitting;
    public static bool Exist => instance != null;
}