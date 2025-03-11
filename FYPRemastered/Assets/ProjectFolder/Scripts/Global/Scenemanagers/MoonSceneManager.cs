using System.Collections.Generic;
using UnityEngine;

public class MoonSceneManager : BaseSceneManager
{
    [SerializeField] private ParticleSystem _normalHitPrefab;
    [SerializeField] private AudioSource _deflectAudioPrefab;
    [SerializeField] private GameObject _normalBulletPrefab;
    //PoolManager _poolManager;

    private void Start()
    {
        SetupScene();

        //_poolManager = new PoolManager(_normalBulletPrefab);
    }

    public override void SetupScene()
    {
        _normalBulletPrefab = Resources.Load<GameObject>("Bullets/NormalBullet");
        //_normalBulletPrefab = bulletPrefab.GetComponentInChildren<BulletCollisionComponent>();
        _deflectAudioPrefab = Resources.Load<AudioSource>("AudioPoolPrefabs/DeflectAudio");

        Dictionary<PoolType, PoolSettings> poolData = new()
        {
            //{PoolType.NormalBullet, new PoolSettings(_normalBulletPrefab, 30, 50, typeof(GameObject)) },
            {PoolType.HitParticle, new PoolSettings(_normalHitPrefab.gameObject, 20, 35, typeof(ParticleSystem)) },
            {PoolType.AudioSRC, new PoolSettings(_deflectAudioPrefab.gameObject, 10, 25, typeof(AudioSource)) }
        };

        ParticlePool.Initialize(poolData, this);
        //ParticlePool.Initialize(_normalHitPrefab, this);
    }
}
