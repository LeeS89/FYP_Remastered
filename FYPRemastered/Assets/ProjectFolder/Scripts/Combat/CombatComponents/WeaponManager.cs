using UnityEngine;

public class WeaponManager : ComponentEvents, IWeaponOwner
{
    protected Weapon _equippedWeapon;

    public GameObject GameObject => gameObject;

    protected bool _isNPC = false;
    public bool IsNPC => _isNPC;

    public Transform Target { get; protected set; } = null;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
        base.RegisterLocalEvents(eventManager);
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(eventManager);
        _eventManager = null;

    }

    protected virtual void TryUseWeapon()
    {
        if (_equippedWeapon == null) return;

        if(_equippedWeapon is IRanged rw) rw.TryFire(FireRate.SingleAutomatic);
        
    }
}
