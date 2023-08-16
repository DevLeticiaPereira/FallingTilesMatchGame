using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : class
{
    private static T singletonInstance;

    public static T Instance
    {
        get
        {
            if (singletonInstance == null) singletonInstance = FindObjectOfType(typeof(T)) as T;
            if (singletonInstance == null) Debug.LogError("No " + typeof(T) + " added to the scene.");
            return singletonInstance;
        }
    }

    protected virtual void Awake()
    {
        if (singletonInstance == null)
        {
            singletonInstance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}