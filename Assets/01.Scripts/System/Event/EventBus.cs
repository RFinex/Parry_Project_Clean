using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> eventHandlers = new Dictionary<Type, Delegate>();

    public static void Subscribe<TEvent>(Action<TEvent> listener)
    {
        if (listener == null)
            return;

        Type eventType = typeof(TEvent);

        if (eventHandlers.TryGetValue(eventType, out Delegate handlers))
        {
            eventHandlers[eventType] = Delegate.Combine(handlers, listener);
        }
        else
        {
            eventHandlers.Add(eventType, listener);
        }
    }

    public static void UnSubscribe<TEvent>(Action<TEvent> listener)
    {
        if (listener == null || !eventHandlers.TryGetValue(typeof(TEvent), out Delegate handlers))
            return;

        handlers = Delegate.Remove(handlers, listener);

        if (handlers == null)
        {
            eventHandlers.Remove(typeof(TEvent));
        }
        else
        {
            eventHandlers[typeof(TEvent)] = handlers;
        }
    }

    public static void Publish<TEvent>(TEvent eventData)
    {
        if (!eventHandlers.TryGetValue(typeof(TEvent), out Delegate handlers))
            return;

        if (handlers is Action<TEvent> action)
        {
            action?.Invoke(eventData);
        }
    }
}
