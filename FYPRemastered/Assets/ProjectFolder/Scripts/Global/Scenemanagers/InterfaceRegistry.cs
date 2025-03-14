using System.Collections.Generic;
using System;
using System.Collections;

public static class InterfaceRegistry
{
    private static Dictionary<Type, IList> _registries = new Dictionary<Type, IList>();
    private static Dictionary<Type, int> _currentIndex = new Dictionary<Type, int>();

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
            // Get the list of objects for this type
            var list = _registries[type] as List<T>;

            // Set the current index for the type to the next available index
            _currentIndex[type] = list.Count;

            return list.AsReadOnly();  // Return the read-only list
        }
        return new List<T>().AsReadOnly();
    }
    /*public static IReadOnlyList<T> GetAll<T>() where T : class
    {
        Type type = typeof(T);
        if (_registries.ContainsKey(type))
        {
            return (_registries[type] as List<T>).AsReadOnly();
        }
        return new List<T>().AsReadOnly();
    }*/


    public static T GetNext<T>() where T : class
    {
        Type type = typeof(T);

        // If we have no objects of this type or we've reached the end, return null
        if (!_registries.ContainsKey(type) || _currentIndex[type] >= _registries[type].Count)
        {
            return null;
        }

        // Get the object at the current index and increment the index
        T obj = _registries[type][_currentIndex[type]] as T;
        _currentIndex[type]++;

        return obj;
    }
}
