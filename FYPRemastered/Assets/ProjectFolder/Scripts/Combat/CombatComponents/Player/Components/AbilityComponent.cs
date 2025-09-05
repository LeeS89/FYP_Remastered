using System.Collections.Generic;
using UnityEngine;

public class AbilityComponent : ComponentEvents, IAbilityOwner
{
    [SerializeField] private AbilityDef _bulletFreeze;
    [SerializeField] private Transform _origin;
    readonly List<AbilityRuntime> _runtimes = new(10);
    public readonly HashSet<AbilityTags> _tags = new(10);

   
    public Transform Origin => _origin ? _origin : transform;
    public AudioSource _audio;

    private Dictionary<StatEntry, float> _resourcesToSpend = new(4);
    public AbilityTags _tag;


    private void Awake()
    {
        if (_bulletFreeze) _runtimes.Add(new AbilityRuntime(_bulletFreeze, this));
    }

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
        base.RegisterLocalEvents(eventManager);
    }

    public bool _tryActivateFreeze = false;
    public bool _tryEndFreeze = false;

    public void TryActivate(AbilityTags id)
    {
        for(int i = 0; i < _runtimes.Count; i++)
        {
            if (_runtimes[i].Id == id)
            {
                _runtimes[i].TryActivate(Time.time);
            }
        }
    }

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

    // Update is called once per frame
    void Update()
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
