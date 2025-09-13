using System;
using UnityEngine;

public sealed class ProjectileEventManager : EventManager
{
    //private List<ComponentEvents> _cachedListeners;
    public event Action OnExpired;
    public event Action OnLaunch;
    public event Action<Collision> OnCollision;
    public event Action<ProjectileKickType> OnDeflected;
    public event Func<Vector3> OnGetDirectionToTarget;
    public event Action OnFreeze;
  //  public event Action OnReverseDirection;
   /* public event Action<Projectile*//*, BulletType*//*> OnBulletParticlePlay;
    public event Action<Projectile*//*, BulletType*//*> OnBulletParticleStop;
    */
    public event Action<bool> OnCull;


    public event Func<Vector3, float, bool> OnSweep;

    public bool Sweep(Vector3 direction, float distance) => OnSweep?.Invoke(direction, distance) ?? false;

    // NEW PARTICLE
    public event Action<ProjectileBase/*, BulletType*/> OnProjectileParticlePlay;
    public event Action<ProjectileBase/*, BulletType*/> OnProjectileParticleStop;


    public event Action<Vector3, Vector3> OnCollide;

    public void ParticleBegin(ProjectileBase proj) => OnProjectileParticlePlay?.Invoke(proj);
    public void ParticleEnd(ProjectileBase proj) => OnProjectileParticleStop?.Invoke(proj);
    // END NEW PARTICLE


    private void Awake() => base.BindComponentsToEvents();

   

    private void Start()
    {
        foreach (var listener in _cachedListeners)
        {
            listener.InitialzeLocalPools();
        }
    }

    public override void UnbindComponentsToEvents()
    {
        foreach(var listener in _cachedListeners)
        {
            listener.UnRegisterLocalEvents(this);
        }
        _cachedListeners?.Clear();
        _cachedListeners = null;
        
    }

    

  /*  public void ParticlePlay(Projectile bullet*//*, BulletType bulletType*//*) => OnBulletParticlePlay?.Invoke(bullet*//*, bulletType*//*);


    public void ParticleStop(Projectile bullet*//*, BulletType bulletType*//*) => OnBulletParticleStop?.Invoke(bullet*//*, bulletType*//*);*/


    public void Cull(bool cull = false) => OnCull?.Invoke(cull);


    public void Launched() => OnLaunch?.Invoke();


    public void Deflected(ProjectileKickType type) => OnDeflected?.Invoke(type);


    public Vector3 GetDirectionToTarget() => OnGetDirectionToTarget?.Invoke() ?? Vector3.zero;


    public void Freeze() => OnFreeze?.Invoke();


    /*public void ReverseDirection()
    {
        OnReverseDirection?.Invoke();
    }*/

    public void Collision(Collision collision) => OnCollision?.Invoke(collision);

    public void Collide(Vector3 hitPoint, Vector3? hitNormal = null) => OnCollide?.Invoke(hitPoint, hitNormal.Value);

    public void Expired() => OnExpired?.Invoke();



}
