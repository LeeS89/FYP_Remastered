using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// To reduce runtime hierarchy lookups with GetComponent<>(), frequently requested components on gameobjects
/// register and store their instance ID and the component to be requested
/// Instead of GetComponent/ TryGetComponent, querying objects
/// call TryGet<T>() passing in the object to query along with the component type to lookup
/// </summary>
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
        /*  _maps.TryGetValue(typeof(IDamageable), out var dict);
          int count = dict?.GetValueOrDefault ?? 0;
          Debug.LogError("IDamageable count: "+count);*/

        // Suppose we add an inner dictionary for a type:
      /*  _maps[typeof(IDamageable)] = new Dictionary<int, object>(100);
        _maps[typeof(IDamageable)][1] = "test";
        _maps[typeof(IDamageable)][2] = "test2";
*/
        // Get the inner dictionary
        var innerDict = _maps[typeof(IDeflectable)];

        // Check before trim
        Debug.LogError($"Count: {innerDict.Count}, Capacity: {innerDict.GetCapacity()}");

        // Trim
        innerDict.TrimExcess();

        // Check after trim
        Debug.LogError($"After TrimExcess -> Count: {innerDict.Count}, Capacity: {innerDict.GetCapacity()}");


        /* foreach (var dict in _maps.Values)
         {
             dict.TrimExcess();
         }*/
    }

    public static int GetCapacity<TKey, TValue>(this Dictionary<TKey, TValue> dict)
    {
        var entriesField = typeof(Dictionary<TKey, TValue>).GetField("_entries",
            BindingFlags.NonPublic | BindingFlags.Instance);

        var entries = (Array?)entriesField?.GetValue(dict);
        return entries?.Length ?? 0;
    }

    public static void clearAll()
    {
        foreach(var d in _maps.Values) d.Clear();
        _maps.Clear();
        _caps.Clear();
    }













   /* public static void Register(GameObject obj, IDamageable damageable)
    {
        _damageables[obj.GetInstanceID()] = damageable;
    }
*/
   /* public static void Unregister(GameObject obj)
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
    }*/
}
