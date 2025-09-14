using UnityEngine;

public class AbilityGesture : BaseGesture
{
    [SerializeField] private Transform _handAnchor;
    [SerializeField] private AbilityTags _abilityId;
    private PlayerEventManager _pEventManager;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        if (eventManager == null) return;

        _pEventManager = eventManager as PlayerEventManager;
        base.RegisterLocalEvents(_pEventManager);
    }

    public override void OnGestureRecognized()
    {
        if (_pEventManager == null) return;
        _pEventManager.TryUseAbility(_abilityId, _handAnchor); 
    }

    public override void OnGestureReleased()
    {
        if (_pEventManager == null) return;
        _pEventManager.EndAbility(_abilityId);
    }

   
}
