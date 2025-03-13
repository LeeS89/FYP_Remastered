using System.Collections.Generic;
using System;
using System.Collections;

public static class InterfaceRegistry
{
    private static Dictionary<Type, IList> _registries = new Dictionary<Type, IList>();

    public static void Add<T>(T obj) where T : class
    {
        Type type = typeof(T);
        if (!_registries.ContainsKey(type))
        {
            _registries[type] = new List<T>();
        }

        List<T> registry = _registries[type] as List<T>;
        if (!registry.Contains(obj))
        {
            registry.Add(obj);
        }
    }

    public static void Remove<T>(T obj) where T : class
    {
        Type type = typeof(T);
        if(_registries.ContainsKey(type))
        {
            List<T> registry = _registries[type] as List<T>;
            registry.Remove(obj);
        }
    }

    public static IReadOnlyList<T> GetAll<T>() where T : class
    {
        Type type = typeof(T);
        if (_registries.ContainsKey(type))
        {
            return (_registries[type] as List<T>).AsReadOnly();
        }
        return new List<T>().AsReadOnly();
    }
}
