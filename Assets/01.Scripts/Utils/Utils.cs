using System.Diagnostics;
using System.Threading;

public static class Utils
{
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Log<T>(string text) where T : class
    {
        UnityEngine.Debug.Log($"[{typeof(T).Name}] {text}");
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning<T>(string text) where T : class
    {
        UnityEngine.Debug.LogWarning($"[{typeof(T).Name}] {text}");
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogError<T>(string text) where T : class
    {
        UnityEngine.Debug.LogError($"[{typeof(T).Name}] {text}");
    }
}
