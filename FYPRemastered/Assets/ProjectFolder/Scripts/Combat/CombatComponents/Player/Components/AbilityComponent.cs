using System.Collections.Generic;
using UnityEngine;

public class AbilityComponent : ComponentEvents, IAbilityOwner
{
    [SerializeField] private AbilityDef _bulletFreeze;
    [SerializeField] private List<AbilityDef> _abilities;
    [SerializeField] private Transform _defaultOrigin;
    private Transform _currentOrigin;
    readonly List<AbilityRuntime> _runtimes = new(10);
    public readonly HashSet<AbilityTags> _tags = new(10);

   
   
    public AudioSource _audio;

    private Dictionary<StatEntry, float> _resourcesToSpend = new(4);
    public AbilityTags _tag;


    private void Awake()
    {
       /* if (_abilities == null || _abilities.Count == 0) return;
        foreach(var ab in _abilities)
        {
            _runtimes.Add(new AbilityRuntime(ab, this));
        }*/
        //if (_bulletFreeze) _runtimes.Add(new AbilityRuntime(_bulletFreeze, this));
    }

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
        base.RegisterLocalEvents(eventManager);

        _eventManager.OnTryUseAbility += TryActivate;
        _eventManager.OnEndAbility += EndChannel;
        ResetOrigin();
        if (_abilities == null || _abilities.Count == 0) return;
        foreach (var ab in _abilities)
        {
            _runtimes.Add(new AbilityRuntime(ab, this));
        }
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _eventManager.OnTryUseAbility -= TryActivate;
        _eventManager.OnEndAbility -= EndChannel;
        base.UnRegisterLocalEvents(eventManager);
       
    }

    public Transform Origin
    {
        get => _currentOrigin;
        set => _currentOrigin = value;
    }

    public Transform GazeOrigin => _defaultOrigin;

    public bool _tryActivateFreeze = false;
    public bool _tryEndFreeze = false;

    public void TryActivate(AbilityTags id, Transform abilityOrigin = null)
    {
        if (OwnerIsDead) return;

        for(int i = 0; i < _runtimes.Count; i++)
        {
            if (_runtimes[i].Id == id)
            {
                if (abilityOrigin != null) Origin = abilityOrigin;
                if(!_runtimes[i].TryActivate(Time.time)) ResetOrigin();
            }
        }
    }

    private void ResetOrigin() => Origin = _defaultOrigin ? _defaultOrigin : transform;


    public void EndChannel(AbilityTags id)
    {
        for(int i = 0; i < _runtimes.Count; i++)
        {
            if (_runtimes[i].Id == id)
            {
                _runtimes[i].End(Time.time);
            }
        }
    }

    protected override void DeathStatusUpdated(bool isDead)
    {
        base.DeathStatusUpdated(isDead);

        if (!OwnerIsDead) return;
        ResetOrigin();
        for (int i = 0; i < _runtimes.Count; i++)
            _runtimes[i].OnInterrupted();
    }

    // Update is called once per frame
    private void Update()
    {
        if (_tryActivateFreeze)
        {
            TryActivate(_tag);
            _tryActivateFreeze = false;
        }
        if (_tryEndFreeze)
        {
            EndChannel(_tag);
            _tryEndFreeze = false;
        }

        float now = Time.time;
        for (int i = 0; i < _runtimes.Count; i++) _runtimes[i].Tick(now);
    }

    private void FixedUpdate()
    {
        float now = Time.fixedTime;
        for(int i = 0; i < _runtimes.Count; i++)
        {
            _runtimes[i].FixedTick(now);
        }
    }

    public void AddTag(AbilityTags tag) => _tags.Add(tag);
    

    public bool HasTag(AbilityTags tag) => _tags.Contains(tag);
   

    public void PlayCue(CueDef cue)
    {
        if (!cue) return;
        if (cue.Sound) AudioSource.PlayClipAtPoint(cue.Sound, Origin.position, cue.Volume);
        if (cue.VfxPrefab)
        {
            var pos = Origin.TransformPoint(cue.VfxOffset);
            var vfx = Instantiate(cue.VfxPrefab, pos, Origin.rotation);
            if (cue.VfxLifetime > 0) Destroy(vfx, cue.VfxLifetime);
        }
    }

    public void RemoveTag(AbilityTags tag) => _tags.Remove(tag);
    

   

    public bool HasSufficientResources(ResourceCost[] costs)
    {
        if (costs == null || costs.Length == 0) return true;

        return _eventManager.CheckIfHasSufficientResources(costs, _resourcesToSpend);
    }

    public void SpendAll(ResourceCost[] costs) => _eventManager.SpendResources(_resourcesToSpend);

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        _resourcesToSpend = null;
        _eventManager = null;
    }

}
