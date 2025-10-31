using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BaseDamageRelay : ComponentEvents, IDamageable
{
    [SerializeField] private Transform _parentTransform;
    private Collider _targetableCollider;
   

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        //base.RegisterLocalEvents(eventManager);
        _eventManager = eventManager;
        _targetableCollider = GetComponent<Collider>();
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

    public bool IsMoving { get; private set; } = false;

   /* public Vector3 GetTargetablePosition()
        => _parentTransform == null ? transform.position : _parentTransform.position;*/

    public Collider GetTargetableCollider() => _targetableCollider;

    public (Vector3, Vector3?) GetTargetablePositionAndForward()
        => _parentTransform == null ? (transform.position, transform.forward) : (_parentTransform.position, _parentTransform.forward);

    public Vector3 GetPosition()
        => _parentTransform == null ? transform.position : _parentTransform.position;
}
