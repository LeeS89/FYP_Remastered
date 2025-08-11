using System.Collections.Generic;
using UnityEngine;

public static class DamageManager
{
    private static readonly Dictionary<int, IDamageable> _damageables = new();

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
