using System;
using UnityEngine;

public class WeaponBase : ComponentEvents
{
    public Transform _spawnPoint;
    public virtual void OnEquipped(AbilityRuntime runtime) { }


}
