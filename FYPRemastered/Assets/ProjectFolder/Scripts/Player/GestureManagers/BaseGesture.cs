using UnityEngine;

public abstract class BaseGesture : ComponentEvents, IPlayerEvents
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


    public abstract void OnSceneStarted();


    public abstract void OnSceneComplete();


    public virtual void OnPlayerDied() { InputEnabled = false; }


    public virtual void OnPlayerRespawned() {  InputEnabled = true; }

    public abstract void RegisterGlobalEvents();


    public abstract void UnRegisterGlobalEvents();
    
}
