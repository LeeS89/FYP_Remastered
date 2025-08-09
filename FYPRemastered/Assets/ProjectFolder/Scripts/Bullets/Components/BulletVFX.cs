using UnityEngine;

public class BulletVFX : ComponentEvents
{
    [SerializeField]
    private ParticleManager _particleManager;
    public ParticleSystem _particle;
    private IPoolManager _particlePoolManager;
    private IPoolManager _audioPoolManager;
    private BulletEventManager _bulletEventManager;

    public ParticleSystem particleMove;

    private ResourceRequest _request;

    //public MoonSceneManager _manager;
    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        if (_eventManager == null) { return; }
        _bulletEventManager = (BulletEventManager)_eventManager;
        
        _particleManager = ParticleManager.instance;
        _bulletEventManager.OnDeflected += PlayDeflectionAudio;
        _bulletEventManager.OnBulletParticlePlay += PlayBulletParticle;
        _bulletEventManager.OnBulletParticleStop += StopBulletParticle;
        _bulletEventManager.OnCollision += SpawnHitParticle;
        //_bulletEventManager.OnSpawnHitParticle += SpawnHitParticle;
        _request = new ResourceRequest();
       
        _request.ResourceType = PoolResourceType.BasicHitParticlePool;
        _request.poolRequestCallback = OnPoolReceived;
        //_request.poolRequestCallback = (pool) =>
        //{

        //        _particlePoolManager = pool;

        //};

        SceneEventAggregator.Instance.RequestResource(_request);

        _request.ResourceType = PoolResourceType.DeflectAudioPool;
      /*  _request.poolRequestCallback = (pool) =>
        {
            _audioPoolManager = pool;
        };*/

        SceneEventAggregator.Instance.RequestResource(_request);
        //BaseSceneManager._instance.GetImpactParticlePool(ref _poolManager);

    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        if (_eventManager == null) { return; }

        _bulletEventManager.Expired();
        _particleManager = null;
        _bulletEventManager.OnDeflected -= PlayDeflectionAudio;
        _bulletEventManager.OnBulletParticlePlay -= PlayBulletParticle;
        _bulletEventManager.OnBulletParticleStop -= StopBulletParticle;
        _bulletEventManager.OnCollision -= SpawnHitParticle;
        //_bulletEventManager.OnSpawnHitParticle -= SpawnHitParticle;
        base.UnRegisterLocalEvents(eventManager);
        _bulletEventManager = null;

        
    }

    private void OnPoolReceived(IPoolManager pool)
    {
        switch (_request.ResourceType)
        {
            case PoolResourceType.BasicHitParticlePool:
                _particlePoolManager = pool;
                break;
                case PoolResourceType.DeflectAudioPool:
                _audioPoolManager = pool;
                break;
        }
        //_audioPoolManager = pool;
    }

    private void PlayDeflectionAudio()
    {
        PoolExtensions.GetAndPlay(_audioPoolManager, transform.position, transform.rotation);
    }
   

    private void SpawnHitParticle(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);
       // ContactPoint contact = collision.contacts[0];
        Vector3 pos = contact.point;
        //Vector3 hitNormal = contact.normal;
        //GameObject obj = _poolManager.GetFromPool(pos, rot);
        PoolExtensions.GetAndPlay(_particlePoolManager, pos, Quaternion.identity);
        //obj.SetActive(true);
        //_poolManager.GetParticle(pos, rot);  
    }

    private void PlayBulletParticle(BulletBase bullet/*, BulletType bulletType*/)
    {
        //ParticleSystem particle = transform.root.GetComponentInChildren<ParticleSystem>();
        

        //particleMove.Clear(true); // Clear old particles
        //particleMove.Play(true);  // Force restart from frame 0

        //particleMove.Play();

        if (_particleManager == null) { return; }    

        _particleManager.AddBullet(bullet/*, bulletType*/);
    }

    private void StopBulletParticle(BulletBase bullet/*, BulletType bulletType*/)
    {
        //ParticleSystem particle = transform.root.GetComponentInChildren<ParticleSystem>();
        /*particleMove.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);*/
        
        //particleMove.Pause();

        if (_particleManager == null) { return; }
        _particleManager.RemoveBullet(bullet);
    }

    private void OnDisable()
    {
        //particleMove.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        //particleMove.transform.localPosition = Vector3.zero;
    }

    /*private void OnEnable()
    {
        particleMove.transform.localPosition = Vector3.zero;
    }*/

   /* private void Update()
    {
        if (particleMove.isPlaying)
        {
            particleMove.transform.localPosition = Vector3.zero;
        }
    }*/

    private bool _justActivated = false;

    void OnEnable()
    {
       // particleMove.Play(true);
        //particleMove.transform.localPosition = Vector3.zero;
        //particleMove.Clear(true);
        //_justActivated = true;
    }

    void LateUpdate()
    {
        /*if (_justActivated)
        {
            //particleMove.Play(true);
            particleMove.transform.localPosition = Vector3.zero;
            _justActivated = false;
        }*/
    }
    /* private void OnDestroy()
     {
         BulletBase bullet = GetComponentInParent<BulletBase>();
         _particleManager.Removebullet(bullet);
     }*/
}
