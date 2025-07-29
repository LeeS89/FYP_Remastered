using System;
using System.Collections;
using UnityEngine;

public static class WeaponHandlerExtensions
{
    public static Coroutine StartSingleFireRoutine(this WeaponHandlerBase weaponHandler, WaitForSeconds shotInterval, Action callback)
    {
        return CoroutineRunner.Instance.StartCoroutine(SingleFireRoutine(shotInterval, callback));
    }

    private static IEnumerator SingleFireRoutine(WaitForSeconds interval, Action callback)
    {
        while (true)
        {
            yield return interval;

            callback?.Invoke();
        }
    }
}
