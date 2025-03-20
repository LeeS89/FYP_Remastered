using UnityEngine;

public abstract class BaseGesture : MonoBehaviour, IComponentEvents, IPlayerEvents
{
    protected bool _inputEnabled = false;

    public abstract void OnGestureRecognized();

    public abstract void OnGestureReleased();

   
    protected virtual void ResetStates() { }

    public abstract void RegisterEvents(EventManager eventManager);


    public abstract void UnRegisterEvents(EventManager eventManager);


    public virtual bool InputEnabled
    {
        set => _inputEnabled = value;
        protected get => _inputEnabled;

    }


    public abstract void OnSceneStarted();


    public abstract void OnSceneComplete();


    public virtual void OnPlayerDied() { InputEnabled = false; }


    public virtual void OnPlayerRespawned() {  InputEnabled = true; }
    

   
}
