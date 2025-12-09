using UnityEngine;

public class BaseDamageRelay : ComponentEvents, IDamageable
{
  //  [SerializeField] private Transform _parentTransform;
   // [SerializeField] private Collider _targetableCollider;
    //public Collider TargetableCollider => _targetableCollider;

   
    public override void RegisterLocalEvents(EventManager eventManager)
    {
        //base.RegisterLocalEvents(eventManager);
        _eventManager = eventManager;
       /* if(_targetableCollider == null)
        {
            if(TryGetComponent<Collider>(out var col))
                _targetableCollider = col;
            else _targetableCollider = gameObject.AddComponent<BoxCollider>();
        }*/
        
    }


    public void Knockback(float damage, Vector3 direction, float force, float duration)
    {
        _eventManager.Knockbacktriggered(direction, force, duration);
        NotifyDamage(damage);
    }

    public void NotifyDamage(float baseDamage, DamageType dType = DamageType.None, float statusEffectChancePercentage = 0, float damageOverTime = 0, float duration = 0)
    {
        if(_eventManager == null)
        {
            Debug.LogError("Event manager not found, please ensure valid event manager");
            return;
        }

        _eventManager.TakeDamage(
            baseDamage,
            dType,
            statusEffectChancePercentage,
            damageOverTime,
            duration
        );
    }

    public bool _testDeath = false;

    

    private void Update()
    {
        if (_testDeath)
        {
            NotifyDamage(1000, DamageType.Normal); // Simulate a death by applying a large amount of damage
            _testDeath = false;
        }
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        _eventManager = null;
    }

  //  public bool IsStationary { get; private set; } = true;

   // public Transform Transform => _parentTransform != null ? _parentTransform : transform;

  //  [SerializeField] protected LayerMask _selfTargetMask;

   // public LayerMask LayerMask => _selfTargetMask;

   // public Vector3 Forward => _parentTransform != null ? _parentTransform.forward : transform.forward;

   // public bool IsDead { get; private set; } = false;


    /* public Vector3 GetTargetablePosition()
         => _parentTransform == null ? transform.position : _parentTransform.position;*/



  /*  public Vector3 Position()
        => _parentTransform == null ? transform.position : _parentTransform.position;

    public Quaternion Rotation()
    {
        throw new System.NotImplementedException();
    }*/
}
