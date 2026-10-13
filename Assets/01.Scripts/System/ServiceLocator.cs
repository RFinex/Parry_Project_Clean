using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public interface IService
{
    bool IsDdol { get; set; }
}

public static class ServiceLocator
{
    static readonly Dictionary<Type, IService> serviceDic = new();    

    public static void Register<T>(IService service)
    {
        if (!serviceDic.TryGetValue(typeof(T), out var value))
        {
            serviceDic[typeof(T)] = service;
        }
    }

    public static void Inject<T>(out T service) where T : IService
    {
        if (serviceDic.TryGetValue(typeof(T), out var value))
        {
            service = (T)value;
        }
        else
        {
            service = default;
        }
    }

    public static bool IsRegister<T>()
    {
        if (serviceDic.ContainsKey(typeof(T)))
        {
            return true;
        }

        return false;
    }

    public static void Remove(Scene scene)
    {
        List<Type> keyList = new List<Type>();

        foreach (var service in serviceDic)
        {
            if (service.Value.IsDdol == false)
            {
                keyList.Add(service.Key);                
            }
        }

        for (int i = 0; i < keyList.Count; i++)
        {
            serviceDic.Remove(keyList[i]);
        }
    }
}
