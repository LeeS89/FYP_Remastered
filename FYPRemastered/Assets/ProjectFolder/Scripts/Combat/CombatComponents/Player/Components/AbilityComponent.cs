using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityComponent : ComponentEvents, IAbilityOwner
{
    // [SerializeField] private AbilityDef _bulletFreeze;
    // [SerializeField] private List<AbilityDef> _abilities;
    [SerializeField] private Transform _defaultAbilityDirectionOrigin;
    private Transform _directionOrigin;

    [SerializeField] private float _abilityFireDirectionOffset = 0f;
    private Transform _abilityFireOrigin;
    readonly List<AbilityRuntime> _runtimes = new(10); // Switch to Dictionary Lookups
    private Dictionary<string, AbilityRuntime> _rt = new(10);
    public readonly HashSet<AbilityTags> _tags = new(10);

    [SerializeField] private List<AbilityParams> _abilityParams;

    public AudioSource _audio;

    private Dictionary<StatEntry, float> _resourcesToSpend = new(4); 
    public AbilityTags _tag; // Delete later
     

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
        if (_abilityParams == null || _abilityParams.Count == 0) return;

        foreach (var ab in _abilityParams)
        {
            ab?.Initialize();
            string id = ab.AbilityId();
            if (string.IsNullOrEmpty(id)) continue;
            AbilityRuntime rt = new AbilityRuntime(ab, this);
            _rt[id] = rt;
        }
       
        _directionOrigin = _defaultAbilityDirectionOrigin ? _defaultAbilityDirectionOrigin : transform;
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _eventManager.OnTryUseAbility -= TryActivate;
        _eventManager.OnEndAbility -= EndChannel;
        base.UnRegisterLocalEvents(eventManager);

    }

    public Transform FireOrigin
    {
        get => _abilityFireOrigin;
        private set => _abilityFireOrigin = value;
    }

    public Transform DirectionOrigin => _defaultAbilityDirectionOrigin;

    public float DirectionOffset => _abilityFireDirectionOffset;

    public bool _tryActivateFreeze = false;
    public bool _tryEndFreeze = false;

    public void TryActivate(AbilityTags tag,Transform abilityFireOrigin = null)
    {
        if (OwnerIsDead) return;


        if (_rt.TryGetValue(tag.Id, out var runtime))
        {
            if (abilityFireOrigin != null) FireOrigin = abilityFireOrigin;
            if (!runtime.TryActivate(Time.time)) ResetOrigin();

        }

    }

    private void ResetOrigin() => FireOrigin = _defaultAbilityDirectionOrigin ? _defaultAbilityDirectionOrigin : transform;


    public void EndChannel(AbilityTags tag)
    {
        string id = tag.Id;
        if (_rt.TryGetValue(id, out var runtime))
        {
            runtime.End(Time.time);
            ResetOrigin();
        }

    }

    protected override void DeathStatusUpdated(bool isDead)
    {
        base.DeathStatusUpdated(isDead);

        if (!OwnerIsDead) return;
        ResetOrigin(); // Switch to stored Currently Active Ability

        foreach (var rt in _rt.Values)
        {
            rt.OnInterrupted();
        }

        /*for (int i = 0; i < _runtimes.Count; i++)
            _runtimes[i].OnInterrupted();*/
    }

    // Update is called once per frame
    private void Update()
    {
        if (_tryActivateFreeze)
        {
            // TryActivate(_tag);
            _tryActivateFreeze = false;
        }
        if (_tryEndFreeze)
        {
            EndChannel(_tag);
            _tryEndFreeze = false;
        }

        float now = Time.time;

        foreach (var rt in _rt.Values) rt.Tick(now);
        //for (int i = 0; i < _rt.Count; i++) _rt.Tick(now);
    }

    private void FixedUpdate()
    {
        float now = Time.fixedTime;
        for (int i = 0; i < _runtimes.Count; i++)
        {
            _runtimes[i].FixedTick(now);
        }
    }

    public void AddTag(AbilityTags tag) => _tags.Add(tag);


    public bool HasTag(AbilityTags tag) => _tags.Contains(tag);


    public void PlayCue(CueDef cue)
    {
        if (!cue) return;
        if (cue.Sound) AudioSource.PlayClipAtPoint(cue.Sound, FireOrigin.position, cue.Volume);
        if (cue.VfxPrefab)
        {
            var pos = FireOrigin.TransformPoint(cue.VfxOffset);
            var vfx = Instantiate(cue.VfxPrefab, pos, FireOrigin.rotation);
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
