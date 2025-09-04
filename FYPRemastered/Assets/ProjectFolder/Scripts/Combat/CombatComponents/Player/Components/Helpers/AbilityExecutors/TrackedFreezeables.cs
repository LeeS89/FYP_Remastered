using System;
using UnityEngine;

public sealed class TrackedFreezeables
{
    private IDeflectable[] _arr = new IDeflectable[64];
    private int _count;

    public bool Add(IDeflectable deflectable)
    {
        for (int i = 0; i < _count; i++) if (ReferenceEquals(_arr[i], deflectable)) return false;
        if (_count == _arr.Length) Array.Resize(ref _arr, _arr.Length * 2);
        _arr[_count++] = deflectable;
        return true;
    }

    public void ClearFreezeables()
    {
        for(int i = 0; i < _count; i++)
        {
            if (_arr[i] != null) _arr[i].Deflect(ProjectileKickType.ReFire);
        }
        Array.Clear(_arr, 0, _count);
        _count = 0;
    }
}
