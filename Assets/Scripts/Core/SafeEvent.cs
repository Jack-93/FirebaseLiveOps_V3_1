using System;
using UnityEngine;

public static class SafeEvent
{
    public static void Invoke(
        Action action,
        string owner,
        string eventName)
    {
        if (action == null)
            return;

        foreach (Delegate handler in action.GetInvocationList())
        {
            try
            {
                ((Action)handler).Invoke();
            }
            catch (Exception exception)
            {
                LogHandlerException(owner, eventName, exception);
            }
        }
    }

    public static void Invoke<T>(
        Action<T> action,
        T value,
        string owner,
        string eventName)
    {
        if (action == null)
            return;

        foreach (Delegate handler in action.GetInvocationList())
        {
            try
            {
                ((Action<T>)handler).Invoke(value);
            }
            catch (Exception exception)
            {
                LogHandlerException(owner, eventName, exception);
            }
        }
    }

    public static void Invoke<T1, T2>(
        Action<T1, T2> action,
        T1 value1,
        T2 value2,
        string owner,
        string eventName)
    {
        if (action == null)
            return;

        foreach (Delegate handler in action.GetInvocationList())
        {
            try
            {
                ((Action<T1, T2>)handler).Invoke(value1, value2);
            }
            catch (Exception exception)
            {
                LogHandlerException(owner, eventName, exception);
            }
        }
    }

    public static void Invoke<T1, T2, T3>(
        Action<T1, T2, T3> action,
        T1 value1,
        T2 value2,
        T3 value3,
        string owner,
        string eventName)
    {
        if (action == null)
            return;

        foreach (Delegate handler in action.GetInvocationList())
        {
            try
            {
                ((Action<T1, T2, T3>)handler).Invoke(
                    value1,
                    value2,
                    value3);
            }
            catch (Exception exception)
            {
                LogHandlerException(owner, eventName, exception);
            }
        }
    }

    private static void LogHandlerException(
        string owner,
        string eventName,
        Exception exception)
    {
        Debug.LogWarning(
            $"[{owner}] {eventName} handler failed: " +
            exception.Message);
        Debug.LogException(exception);
    }
}
