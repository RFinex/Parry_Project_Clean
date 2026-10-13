using System.Collections.Generic;
using System;

public static class ServiceLocator
{
    static readonly Dictionary<Type, object> serviceDic = new();

    public static void Register<T>(object service)
    {
        if (!serviceDic.TryGetValue(typeof(T), out var value))
        {
            serviceDic[typeof(T)] = service;
        }
    }

    public static void Inject<T>(out T service)
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
}
