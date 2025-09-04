using System.Collections.Generic;
using UnityEngine;

public class AbilityComponent : ComponentEvents, IAbilityOwner
{
    [SerializeField] private AbilityDef _bulletFreeze;
    [SerializeField] private Transform _origin;
    readonly List<AbilityRuntime> _runtimes = new();
    readonly HashSet<AbilityTags> _tags = new();
    //EventManager manager;
    public Transform Origin => _origin ? _origin : transform;
    public AudioSource _audio;

    private void Awake()
    {
        if (_bulletFreeze) _runtimes.Add(new AbilityRuntime(_bulletFreeze, this));
    }

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
        base.RegisterLocalEvents(eventManager);
    }

    

    // Update is called once per frame
    void Update()
    {
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

        for(int i =0; i < costs.Length; i++)
        {

        }
       // _eventManager.SpendResource();
        return true;
    }

    public void SpendAll(ResourceCost[] costs)
    {
       // throw new System.NotImplementedException();
    }
}
