using UnityEngine;

public class MoonSceneManager : BaseSceneManager
{
    [SerializeField] private ParticleSystem _normalHitPrefab;

    private void Start()
    {
        SetupScene();
    }

    public override void SetupScene()
    {
        ParticlePool.Initialize(_normalHitPrefab, this);
    }
}
