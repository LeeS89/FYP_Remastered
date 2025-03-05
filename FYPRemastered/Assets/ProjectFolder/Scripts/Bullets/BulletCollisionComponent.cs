using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletCollisionComponent : MonoBehaviour, IBulletEvents
{
    private BulletEventManager _eventManager;

    public void RegisterEvents(BulletEventManager eventManager)
    {
        if(eventManager == null) { return; }
        _eventManager = eventManager;
    }


    private void OnCollisionEnter(Collision collision)
    {

        _eventManager.Expired();

    }

   
}
