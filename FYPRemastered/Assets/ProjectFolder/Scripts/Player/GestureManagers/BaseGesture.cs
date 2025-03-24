using UnityEngine;

public abstract class BaseGesture : ComponentEvents//, IPlayerEvents
{
    protected bool _inputEnabled = false;

    public abstract void OnGestureRecognized();

    public abstract void OnGestureReleased();

   
    protected virtual void ResetStates() { }

   /* public abstract override void RegisterLocalEvents(EventManager eventManager);


    public abstract void UnRegisterLocalEvents(EventManager eventManager);
*/

    public virtual bool InputEnabled
    {
        set => _inputEnabled = value;
        protected get => _inputEnabled;

    }


    protected override void OnSceneStarted() { }


    protected override void OnSceneComplete() { }


    protected override void OnPlayerDied() { InputEnabled = false; }


    protected override void OnPlayerRespawned() {  InputEnabled = true; }

    protected override void RegisterGlobalEvents() { }


    protected override void UnRegisterGlobalEvents() { }
    
}
