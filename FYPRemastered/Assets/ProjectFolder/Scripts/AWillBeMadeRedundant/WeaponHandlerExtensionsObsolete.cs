using System;
using System.Collections;
using UnityEngine;

public static class WeaponHandlerExtensionsObsolete
{
    public static Coroutine StartSingleFireRoutine(this WeaponHandlerBaseObsolete weaponHandler, WaitForSeconds shotInterval/*, Action callback*/)
    {
        return CoroutineRunner.Instance.StartCoroutine(SingleFireRoutine(weaponHandler, shotInterval/*, callback*/));
    }

    private static IEnumerator SingleFireRoutine(WeaponHandlerBaseObsolete weaponHandler, WaitForSeconds interval/*, Action callback*/)
    {
        while (true)
        {
            yield return interval;

            weaponHandler.TryFireRangedWeapon();
            //callback?.Invoke();
        }
    }
}
