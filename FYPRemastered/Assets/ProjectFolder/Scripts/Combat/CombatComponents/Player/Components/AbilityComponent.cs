using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityComponent : ComponentEvents, IAbilityOwner
{
    [SerializeField] private AbilityOrigins _origins = new();

   // [SerializeField] private List<AbilityParam> _abilityPools;


  //  [SerializeField] private List<AbilityDef> _abilities;
    [SerializeField] private List<AbilityDefinition> _abilities;
  //  [SerializeField] private Transform _defaultAbilityDirectionOrigin;
    //private Transform _directionOrigin;


  
    // private List<AbilityRuntime> _runtimeUpdates = new(10); // Switch to Dictionary Lookups
    private List<AbilityManager> _runtimeUpdates = new(10); // Switch to Dictionary Lookups
    private Dictionary<string, AbilityManager> _runtimeLookups = new(10);
   // private Dictionary<string, AbilityRuntime> _runtimeLookups = new(10);
    public readonly HashSet<AbilityTags> _tags = new(10);

   // [SerializeField] private List<AbilityParams> _abilityParams;

    public AudioSource _audio;

   // private Dictionary<StatEntry, float> _resourcesToSpend = new(4); 
    public AbilityTags _tag; // Delete later
  

   

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
        base.RegisterLocalEvents(eventManager);

        _eventManager.OnTryUseAbility += TryActivate;
        _eventManager.OnEndAbility += EndChannel;

        InitializeAbilityOrigins();
        
    }

    private void InitializeAbilityOrigins()
    {
        if (_origins.Position == null)
        {
#if UNITY_EDITOR
            throw new NullReferenceException("Must provide An ability Origin");
#else
            return;
#endif

        }
       /* ExecuteOrigin = _origins.Position;
        DirectionOrigin = _origins.DirectionOrigin;
        DirectionOffset = _origins.DirectionOffset;*/
        InitializeRuntimes();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _eventManager.OnTryUseAbility -= TryActivate;
        _eventManager.OnEndAbility -= EndChannel;
        base.UnRegisterLocalEvents(eventManager);

    }

    private void InitializeRuntimes()
    {
        if (_abilities == null || _abilities.Count == 0) return;
        foreach (var ab in _abilities)
        {
            string id = ab.Tag.Id;
            if (id == null) continue;
            id.Trim();
            if (id.Length == 0) continue;

          //  AbilityRuntime rt = new AbilityRuntime(ab, this);
            AbilityManager rt = new AbilityManager(ab, this);
            if (_runtimeLookups.TryAdd(id, rt))
            {
                _runtimeUpdates.Add(rt);
            }

        }
    }

    public Transform ExecuteOrigin { get; private set; }

   
    public Transform DirectionOrigin { get; private set; }
  
    public float DirectionOffset { get; private set; }

    public bool _tryActivateFreeze = false;
    public bool _tryEndFreeze = false;



    public void TryActivate(AbilityTags tag, AbilityOrigins origins = null) // NEWEST
    {
        if (OwnerIsDead) return;


        if (_runtimeLookups.TryGetValue(tag.Id, out var runtime))
        {
            // if (abilityFireOrigin != null) ExecuteOrigin = abilityFireOrigin;
            //if (!runtime.TryActivate(Time.time)) ResetOrigin();
            runtime.TryActivate(Time.time, origins);
        }

    }

    public void TryActivate(AbilityTags tag,Transform abilityFireOrigin = null)
    {
        if (OwnerIsDead) return;


        if (_runtimeLookups.TryGetValue(tag.Id, out var runtime))
        {
            if (abilityFireOrigin != null) ExecuteOrigin = abilityFireOrigin;
           // if (!runtime.TryActivate(Time.time)) ResetOrigin();

        }

    }

  /*  public void TryActivate(AbilityParam abParams, Transform activateOrigin = null, Vector3? direction = null)
    {
        if (OwnerIsDead) return;

        string tag = abParams.AbilityIdTag.Id;

        if(_runtimeLookups.TryGetValue(tag, out var rt))
        {

        }
    }*/

  


    public void EndChannel(AbilityTags tag)
    {
        string id = tag.Id;
        if (_runtimeLookups.TryGetValue(id, out var runtime))
        {
            runtime.End(Time.time);
           // ResetOrigin();
        }

    }

    protected override void DeathStatusUpdated(bool isDead)
    {
        base.DeathStatusUpdated(isDead);

        if (!OwnerIsDead) return;
     //   ResetOrigin(); // Switch to stored Currently Active Ability

        foreach (var rt in _runtimeLookups.Values)
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
             TryActivate(_tag, _origins);
            _tryActivateFreeze = false;
        }
        if (_tryEndFreeze)
        {
            EndChannel(_tag);
            _tryEndFreeze = false;
        }

        float now = Time.time;

        foreach (var rt in _runtimeLookups.Values) rt.Tick(now);
        //for (int i = 0; i < _rt.Count; i++) _rt.Tick(now);
    }

    private void FixedUpdate()
    {
        float now = Time.fixedTime;
        /*foreach(var rt in _runtimeLookups.Values)
        {
            rt.FixedTick(now);
        }*/

        for (int i = 0; i < _runtimeUpdates.Count; i++)
        {
            _runtimeUpdates[i].FixedTick(now);
        }
    }

    public void AddTag(AbilityTags tag) => _tags.Add(tag);


    public bool HasTag(AbilityTags tag) => _tags.Contains(tag);


    public void PlayCue(CueDef cue)
    {
        if (!cue) return;
        if (cue.Sound) AudioSource.PlayClipAtPoint(cue.Sound, ExecuteOrigin.position, cue.Volume);
        if (cue.VfxPrefab)
        {
            var pos = ExecuteOrigin.TransformPoint(cue.VfxOffset);
            var vfx = Instantiate(cue.VfxPrefab, pos, ExecuteOrigin.rotation);
            if (cue.VfxLifetime > 0) Destroy(vfx, cue.VfxLifetime);
        }
    }

    public void RemoveTag(AbilityTags tag) => _tags.Remove(tag);




    public bool HasSufficientResources(ResourceCost cost)
    {
        if (cost.ResourceType == StatType.None || cost.Amount <= 0) return true;
       // if (cost == null || costs.Length == 0) return true;

        return _eventManager.CheckIfHasSufficientResources(cost/*, _resourcesToSpend*/);
    }

    public void Spend(ResourceCost cost) => _eventManager.SpendResources(cost/*_resourcesToSpend*/);

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
      //  _resourcesToSpend = null;
        _eventManager = null;
    }

  
}
