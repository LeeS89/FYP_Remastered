using System;
using System.Collections;
using UnityEngine;
[Obsolete]
public sealed class TrackedFreezeables
{
    private IFreezeAndDeflectable[] _arr = new IFreezeAndDeflectable[64];
    private int _count;

    public bool Add(IFreezeAndDeflectable deflectable)
    {
        for (int i = 0; i < _count; i++) if (ReferenceEquals(_arr[i], deflectable)) return false;
        if (_count == _arr.Length) Array.Resize(ref _arr, _arr.Length * 2);
        _arr[_count++] = deflectable;
        return true;
    }

    public void ClearFreezeables()
    {
        if (_arr == null || _arr.Length == 0) return;

        CoroutineRunner.Instance.StartCoroutine(FireFreezablesDelay());
        /* for(int i = 0; i < _count; i++)
         {
             if (_arr[i] != null) _arr[i].Deflect(ProjectileKickType.ReFire);
         }
         Array.Clear(_arr, 0, _count);
         _count = 0;*/
    }


    private IEnumerator FireFreezablesDelay()
    {
        //if (_arr == null || _arr.Length == 0) yield break;

        for (int i = 0; i < _arr.Length; i++)
        {
            var d = _arr[i];
            if (d != null)
            {
                d.Deflect(ProjectileKickType.ReFire);
                yield return new WaitForSeconds(0.1f);
            }
            
        }
        Array.Clear(_arr, 0, _count);
        _count = 0;
    }

}
