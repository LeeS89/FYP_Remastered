using System;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalServices
{
    private static readonly Dictionary<Type, object> _services = new();
    private static readonly Dictionary<Type, int> _refCounts = new();

    /// <summary>
    /// srga
    /// </summary>
    /// <typeparam name="T">dx</typeparam>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static T Acquire<T>(Func<T> factory) where T : IGlobalService
    {
        var type = typeof(T);

        if (_services.TryGetValue(type, out var existing))
        {
            _refCounts[type]++;
            DebugLogs.Log($"Service of type {type} already acquired. Incrementing reference count to {_refCounts[type]}.");
            return (T)existing;
        }

        var created = factory();
        _services[type] = created;
        _refCounts[type] = 1;
        return created;

    }

    public static void Release<T>() where T : IGlobalService
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var existing))
        {
            _refCounts[type]--;
            if (_refCounts[type] <= 0)
            {
                if (existing is IDisposable disposable)
                    disposable.Dispose();

                _services.Remove(type);
                _refCounts.Remove(type);
            }
        }
        else
        {
            DebugLogs.Warn($"Attempted to release service of type {type} that was not acquired.");
        }
    }
}

public interface IGlobalService { }
