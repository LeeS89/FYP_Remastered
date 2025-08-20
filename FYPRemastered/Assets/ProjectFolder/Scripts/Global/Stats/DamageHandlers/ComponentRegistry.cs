using System;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentRegistry
{
    private static readonly Dictionary<int, IDamageable> _damageables = new();

    private static readonly Dictionary<Type, Dictionary<int, object>> _maps = new();
    private static readonly Dictionary<Type, int> _caps = new();

    public static void SetCapacities<T>(int capacity) where T : class
     => _caps[typeof(T)] = Mathf.Max(0, capacity);

    private static Dictionary<int, object> GetOrCreate(Type type)
    {
        if(_maps.TryGetValue(type, out var dict)) return dict;

        if(_caps.TryGetValue(type, out var cap) && cap > 0)
        {
            dict = new Dictionary<int, object>(cap);
        }
        else
        {
            dict = new Dictionary<int, object>();
        }
        _maps[type] = dict;
        return dict;
    }

    public static void Register<T>(GameObject obj, T comp) where T : class
     => GetOrCreate(typeof(T))[obj.GetInstanceID()] = comp;
    
    public static void Unregister<T>(GameObject obj) where T : class
    {
        if (_maps.TryGetValue(typeof(T), out var dict))
        {
            dict.Remove(obj.GetInstanceID());
        }
    }

    /// <summary>
    /// Returns the component of type T from the passed in GameObject.
    /// First checks the dictionary contains the type T,
    /// then searches the inner dictionary of type T for the GameObject's instance ID.
    /// </summary>
    /// <typeparam name="T"> Component Requested</typeparam>
    /// <param name="obj">GameObject to query</param>
    /// <param name="comp">The returned component</param>
    /// <returns>Returns true if match found</returns>
    public static bool TryGet<T>(GameObject obj, out T comp) where T : class
    {
        comp = null;
        return _maps.TryGetValue(typeof(T), out var dict)
            && dict.TryGetValue(obj.GetInstanceID(), out var go)
            && (comp = go as T) != null;
    }

    public static void TrimAll()
    {
        foreach(var dict in _maps.Values)
        {
            dict.TrimExcess();
        }
    }

    public static void clearAll()
    {
        foreach(var d in _maps.Values) d.Clear();
        _maps.Clear();
        _caps.Clear();
    }













    public static void Register(GameObject obj, IDamageable damageable)
    {
        _damageables[obj.GetInstanceID()] = damageable;
    }

    public static void Unregister(GameObject obj)
    {
        _damageables.Remove(obj.GetInstanceID());
    }

    public static bool TryGetDamageable(GameObject obj, out IDamageable damageable)
    {
        return _damageables.TryGetValue(obj.GetInstanceID(), out damageable);
    }

    public static void Clear()
    {
        _damageables.Clear();
    }
}
